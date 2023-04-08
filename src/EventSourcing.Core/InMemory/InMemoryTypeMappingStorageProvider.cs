using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Types;

namespace EventSourcing.Core.InMemory;

public class InMemoryTypeMappingStorageProvider : ITypeMappingStorageProvider
{
    private readonly List<TypeMapping> _mappings = new List<TypeMapping>();

    public Task Initialize()
    {
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<TypeMapping>> GetMappings()
    {
        lock (_mappings)
        {
            IReadOnlyCollection<TypeMapping> copy = _mappings.ToArray();
            return Task.FromResult(copy);
        }
    }

    public Task AddMappings(IEnumerable<TypeMapping> mappings)
    {
        lock (_mappings)
        {
            _mappings.AddRange(mappings);
            return Task.CompletedTask;
        }
    }
}