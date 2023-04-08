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

public class TypeMappingHandler : ITypeMappingHandler
{
    private readonly ITypeMappingStorageProvider _storageProvider;

    private readonly ConcurrentDictionary<TypeMappingId, Type> _mappings = new();

    private bool _storageTypesLoaded = false;

    public TypeMappingHandler(ITypeMappingStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }

    public Type GetType(TypeMappingId id)
    {
        if (!_mappings.TryGetValue(id, out var type))
        {
            Thrown.InvalidOperationException($"Couldn't find type for mapping id '{id.ToString()}'");
        }

        return type;
    }

    public TypeMappingId GetId(Type type)
    {
        throw new NotImplementedException();
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

    internal async Task LoadAppTypes(IEnumerable<Type> types)
    {
        if (!_storageTypesLoaded)
        {
            await LoadFromStorage();
        }

        HashSet<Type> hash = _mappings.Select(x => x.Value).ToHashSet();

        List<TypeMapping> mappings = new List<TypeMapping>();
        foreach (Type type in types)
        {
            bool hasMapping = hash.Contains(type);
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
}