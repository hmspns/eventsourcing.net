using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Abstractions.Types;
using EventSourcing.Core.Exceptions;

namespace EventSourcing.Core.Implementations;

public class TypeMappingHandler : ITypeMappingHandler
{
    private ITypeMappingStorageProvider _storageProvider;

    private BidirectionalMapping<TypeMappingId, Type> _mappings = new();

    private bool _isStorageTypesSynchronized = false;
    private Type[] _storageTypes;
    private bool _isDisposed = false;
    private readonly ManualResetEventSlim _manualResetEvent = new(true);

    public TypeMappingHandler(ITypeMappingStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }

    public Type GetTypeById(TypeMappingId id)
    {
        CheckDisposed();
        if (!_mappings.TryGetValue(id, out Type? type))
        {
            Thrown.InvalidOperationException($"Couldn't find type for mapping id '{id.ToString()}'");
        }

        return type;
    }

    public TypeMappingId GetIdByType(Type type)
    {
        CheckDisposed();
        if (!_manualResetEvent.Wait(TimeSpan.FromSeconds(10)))
        {
            Thrown.InvalidOperationException($"Couldn't get id for type {type.ToString()} because of time out");    
        }
        
        if (!_mappings.TryGetValue(type, out TypeMappingId id))
        {
            try
            {
                _manualResetEvent.Reset();
                id = TypeMappingId.New();
                if (!AddTypeInternal(id, type)) // we are in concurrent race and trying to add the same type at the same moment with the different id, but it isn't possible
                {
                    bool idFound = false;
                    int count = 10;
                    do
                    {
                        Thread.Sleep(20);
                        if (_mappings.TryGetValue(type, out id))
                        {
                            idFound = true;
                            break;
                        }
                    } while (count-- > 0);

                    if (!idFound)
                    {
                        Thrown.InvalidOperationException($"Couldn't add mapping for type {type.ToString()}");
                    }
                }
            }
            finally
            {
                _manualResetEvent.Set();
            }
        }

        return id;
    }

    void ITypeMappingHandler.SetStorageTypes(IEnumerable<Type> types)
    {
        CheckDisposed();
        _storageTypes = types.ToArray();
    }
    
    /// <summary>
    /// Load 
    /// </summary>
    /// <param name="types"></param>
    async Task ITypeMappingHandler.SynchronizeAppTypesWithStorageTypes()
    {
        CheckDisposed();
        if (_isStorageTypesSynchronized)
        {
            return;
        }
        await LoadFromStorage().ConfigureAwait(false);

        List<TypeMapping> mappings = new List<TypeMapping>();
        foreach (Type type in _storageTypes)
        {
            bool hasMapping = _mappings.TryGetValue(type, out var id);
            if (!hasMapping)
            {
                mappings.Add(new TypeMapping(TypeMappingId.New(), GetStringRepresentation(type)));
            }
        }

        if (mappings.Any())
        {
            await _storageProvider.AddMappings(mappings).ConfigureAwait(false);
        }
        
        _storageTypes = null; // we don't need it anymore.
        _isStorageTypesSynchronized = true;
    }

    private bool AddTypeInternal(TypeMappingId id, Type type)
    {
        if (_mappings.TryAdd(id, type))
        {
            try
            {
                TypeMapping mapping = new TypeMapping(id, GetStringRepresentation(type));
                _storageProvider.AddMappings(new []{ mapping}).GetAwaiter().GetResult();
                return true;
            }
            catch
            {
                _mappings.TryRemove(id, type); // something went wrong during adding to storage, we should remove unprocessed mappings.
                throw;
            }
        }

        return false;
    }

    private async Task LoadFromStorage()
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
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (disposing)
        {
            _mappings.Dispose();
            _manualResetEvent.Dispose();
        }

        _mappings = null;
        _storageProvider = null;
        _storageTypes = null;

        _isDisposed = true;
    }

    private void CheckDisposed()
    {
        if (_isDisposed)
        {
            Thrown.ObjectDisposedException(nameof(TypeMappingHandler));
        }
    }
}