using System.Threading.Tasks;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Core.InMemory;

public class InMemoryEventSourcingStorage : IEventSourcingStorage
{
    public Task Initialize(TenantId tenantId)
    {
        return Task.CompletedTask;
    }
}