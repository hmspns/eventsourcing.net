using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Abstractions.Types;
using EventSourcing.Core.Exceptions;

namespace EventSourcing.Core.Implementations;

public class TypeMappingHandler : ITypeMappingHandler, IDisposable
{
    private readonly ITypeMappingStorageProvider _storageProvider;

    private readonly BidirectionalMapping<TypeMappingId, Type> _mappings = new();

    private bool _storageTypesLoaded = false;

    public TypeMappingHandler(ITypeMappingStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }

    public Type GetType(TypeMappingId id)
    {
        if (!_mappings.TryGetValue(id, out Type? type))
        {
            Thrown.InvalidOperationException($"Couldn't find type for mapping id '{id.ToString()}'");
        }

        return type;
    }

    public TypeMappingId GetId(Type type)
    {
        if (!_mappings.TryGetValue(type, out TypeMappingId id))
        {
            id = TypeMappingId.New();
            AddTypeInternal(id, type);
        }

        return id;
    }

    private void AddTypeInternal(TypeMappingId id, Type type)
    {
        if (_mappings.TryAdd(id, type))
        {
            TypeMapping mapping = new TypeMapping(id, GetStringRepresentation(type));
            _storageProvider.AddMappings(new []{ mapping}).GetAwaiter().GetResult();
        }
    }

    internal async Task LoadFromStorage()
    {
        IReadOnlyCollection<TypeMapping> types = await _storageProvider.GetMappings().ConfigureAwait(false);
        foreach (TypeMapping mapping in types)
        {
            try
            {
                Type type = Type.GetType(mapping.TypeName)!;
                if (!_mappings.TryAdd(mapping.Id, type))
                {
                    Thrown.InvalidOperationException($"Type with id '{mapping.Id.ToString()}' already added. It means that storage contains duplicate type mapping entries");
                }
            }
            catch (TypeLoadException e)
            {
                Thrown.InvalidOperationException($@"
Couldn't find proper type for '{mapping.TypeName}' that present in storage.
Probably it was removed from source code. It might cause errors during deserializing events.
If type was removed on purpose remove it's mapping from storage", e);
            }
        }

        _storageTypesLoaded = true;
    }

    /// <summary>
    /// Load 
    /// </summary>
    /// <param name="types"></param>
    internal async Task SynchronizeAppTypesWithStorageTypes(IEnumerable<Type> types)
    {
        if (!_storageTypesLoaded)
        {
            await LoadFromStorage();
        }

        List<TypeMapping> mappings = new List<TypeMapping>();
        foreach (Type type in types)
        {
            bool hasMapping = _mappings.TryGetValue(type, out var id);
            if (!hasMapping)
            {
                mappings.Add(new TypeMapping(TypeMappingId.New(), GetStringRepresentation(type)));
            }
        }

        if (mappings.Any())
        {
            await _storageProvider.AddMappings(mappings);
        }
    }

    /// <summary>
    /// Get string representation of type to store it in the storage.
    /// </summary>
    /// <param name="type">Type to get its string representation.</param>
    /// <returns>String representation of type.</returns>
    /// <exception cref="InvalidOperationException">Full name of <paramref name="type"/> returns null.</exception>
    protected virtual string GetStringRepresentation(Type type)
    {
        string? representation = type.FullName;
        if (representation == null)
        {
            Thrown.InvalidOperationException($"Property FullName of type '{type.ToString()}' return null");
        }
        return representation;
    }

    public void Dispose()
    {
        _mappings.Dispose();
    }
}