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
    /// <remarks>Command source is the place where command was sent.</remarks>
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
        return await _sender.Send(envelope, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Send command to handler.
    /// </summary>
    /// <param name="commandEnvelope">Command envelope that will be sent to handler.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <typeparam name="TId">Type of aggregate id.</typeparam>
    /// <typeparam name="TPayload">Type of command payload.</typeparam>
    /// <returns>Result of command execution.</returns>
    /// <exception cref="InvalidOperationException">Handler not registered.</exception>
    /// <remarks>
    /// Command source is the place where command was sent.
    /// 
    /// It's important to have a good performance use a specific type of TPayload instead of general ICommand.
    /// When ICommand passed as TPayload bus has to use reflection to find the proper handler and create command envelope.
    /// When a specific type passed as TPayload reflection not needed.
    /// </remarks>
    public async Task<ICommandExecutionResult<TId>> Send<TId, TPayload>(ICommandEnvelope<TId> commandEnvelope, CancellationToken cancellationToken = default)
        where TPayload : ICommand
    {
        MediatrCommandEnvelope<TId, TPayload> envelope = new MediatrCommandEnvelope<TId, TPayload>()
        {
            Payload = (TPayload)commandEnvelope.Payload,
            Source = commandEnvelope.Source,
            Timestamp = commandEnvelope.Timestamp,
            AggregateId = commandEnvelope.AggregateId,
            CommandId = commandEnvelope.CommandId,
            PrincipalId = commandEnvelope.PrincipalId,
            SequenceId = commandEnvelope.SequenceId,
            TenantId = commandEnvelope.TenantId,
            ParentCommandId = commandEnvelope.ParentCommandId
        };
        return await _sender.Send(envelope, cancellationToken).ConfigureAwait(false);
    }

    public IPublicationAwaiter PublicationAwaiter { get => new InMemoryPublicationAwaiter(); }
}