using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Abstractions;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Core.Contracts;

namespace EventSourcing.Core.InMemory;

public sealed class InMemoryCommandBus : IEventSourcingCommandBus
{
    public IPublicationAwaiter PublicationAwaiter { get; }

    public Task<ICommandExecutionResult<TId>> Send<TId, TPayload>(TId id, TPayload command, CancellationToken cancellationToken = default) where TPayload : ICommand
    {
        throw new System.NotImplementedException();
    }

    public Task<ICommandExecutionResult<TId>> Send<TId, TPayload>(TenantId tenantId, PrincipalId principalId, string source,
        TId aggregateId, TPayload commandPayload, CancellationToken cancellationToken = default) where TPayload : ICommand
    {
        throw new System.NotImplementedException();
    }
}