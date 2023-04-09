using System.Threading.Tasks;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Core.InMemory;

public class InMemoryEventSourcingStorage : IEventSourcingStorage
{
    public Task Initialize(TenantId tenantId)
    {
        return Task.CompletedTask;
    }
}