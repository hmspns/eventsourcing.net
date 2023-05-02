using System.Threading.Tasks;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;

namespace EventSourcing.Net.Engine.InMemory;

public class InMemoryEventSourcingStorage : IEventSourcingStorage
{
    public Task Initialize(TenantId tenantId)
    {
        return Task.CompletedTask;
    }
}