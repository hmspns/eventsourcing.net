using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using MediatR;

namespace EventSourcing.Net.Bus.Mediatr;

/// <inheritdoc />
public sealed class MediatrEventSourcingCommandBus : IEventSourcingCommandBus
{
    private readonly ISender _sender;

    public MediatrEventSourcingCommandBus(ISender sender)
    {
        _sender = sender;
    }
    
    /// <summary>
    /// Send message to bus.
    /// </summary>
    /// <param name="tenantId">Tenant id.</param>
    /// <param name="principalId">Principal id.</param>
    /// <param name="source">Command source.</param>
    /// <param name="aggregateId">Aggregate id.</param>
    /// <param name="commandPayload">Command data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TId">Aggregate id type.</typeparam>
    /// <typeparam name="TPayload">Command payload type.</typeparam>
    /// <returns>Command execution result.</returns>
    public async Task<ICommandExecutionResult<TId>> Send<TId, TPayload>(
        TenantId tenantId,
        PrincipalId principalId,
        string source,
        TId aggregateId,
        TPayload commandPayload,
        CancellationToken cancellationToken = default
    ) where TPayload : ICommand
    {
        MediatrCommandEnvelope<TId, TPayload> envelope = new MediatrCommandEnvelope<TId, TPayload>()
        {
            Payload = commandPayload,
            Source = source,
            PrincipalId = principalId,
            Timestamp = DateTime.UtcNow,
            AggregateId = aggregateId,
            CommandId = CommandId.New(),
            SequenceId = CommandSequenceId.New(),
            TenantId = tenantId,
            ParentCommandId = CommandId.Empty
        };
        return await _sender.Send(envelope, cancellationToken);
    }

    public IPublicationAwaiter PublicationAwaiter { get; }
}