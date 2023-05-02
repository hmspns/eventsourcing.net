using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Types;

namespace EventSourcing.Net.Core.InMemory;

public class InMemoryTypeMappingStorageProvider : ITypeMappingStorageProvider
{
    private readonly ConcurrentBag<TypeMapping> _mappings = new ConcurrentBag<TypeMapping>();

    public Task Initialize()
    {
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<TypeMapping>> GetMappings()
    {
        IReadOnlyCollection<TypeMapping> copy = _mappings.ToArray();
        return Task.FromResult(copy);
    }

    public Task AddMappings(IEnumerable<TypeMapping> mappings)
    {
        foreach (TypeMapping mapping in mappings)
        {
            _mappings.Add(mapping);
        }
        return Task.CompletedTask;
    }
}