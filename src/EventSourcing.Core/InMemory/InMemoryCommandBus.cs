using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Abstractions;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Core.Contracts;

namespace EventSourcing.Core.InMemory;

public class InMemoryCommandBus : IEventSourcingCommandBus
{
    public Task<ICommandExecutionResult<TId>> Send<TId, TPayload>(TenantId tenantId, PrincipalId principalId, string source, TId aggregateId,
        TPayload commandPayload, CancellationToken cancellationToken = default) where TPayload : ICommand
    {
        throw new System.NotImplementedException();
    }

    public IPublicationAwaiter PublicationAwaiter { get; }
}