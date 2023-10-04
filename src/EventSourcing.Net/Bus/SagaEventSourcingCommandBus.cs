namespace EventSourcing.Net;

using Abstractions.Contracts;

/// <summary>
/// Built in saga command bus.
/// </summary>
public sealed class SagaEventSourcingCommandBus : ISagaEventSourcingCommandBus
{
    private readonly IEventSourcingCommandBus _innerBus;

    public SagaEventSourcingCommandBus(IEventSourcingCommandBus innerBus)
    {
        _innerBus = innerBus;
    }

    /// <summary>
    /// Send command to handler.
    /// </summary>
    /// <param name="source">Command source.</param>
    /// <param name="aggregateId">Aggregate id.</param>
    /// <param name="commandPayload">Command payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
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
    public Task<ICommandExecutionResult<TId>> Send<TId, TPayload>(TId aggregateId, IEventEnvelope originalEvent, TPayload commandPayload, string source = null, CancellationToken cancellationToken = default)
        where TPayload : ICommand
    {
        if (source == null)
        {
            source = string.Empty;
        }
        
        ICommandEnvelope<TId> commandEnvelope = CommandEnvelopeBuilder.ToEnvelope(originalEvent.TenantId, originalEvent.PrincipalId, source, aggregateId, commandPayload);
        IInitializableCommandEnvelope initializable = (IInitializableCommandEnvelope)commandEnvelope;
        initializable.ParentCommandId = originalEvent.CommandId;
        initializable.SequenceId = originalEvent.SequenceId;

        return _innerBus.Send<TId, TPayload>(commandEnvelope, cancellationToken);
    }
}