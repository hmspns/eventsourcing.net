namespace EventSourcing.Net;

using Abstractions.Contracts;
using Internal;

public sealed class SagaEventSourcingCommandBus
{
    private readonly IEventSourcingCommandBus _innerBus;

    public SagaEventSourcingCommandBus(IEventSourcingCommandBus innerBus)
    {
        _innerBus = innerBus;
    }
    
    public Task Send<TId, TPayload>(TId aggregateId, IEventEnvelope originalEvent, TPayload payload, string source = null, CancellationToken cancellationToken = default)
        where TPayload : ICommand
    {
        ICommandEnvelope<TId> commandEnvelope = CommandEnvelopeBuilder.ToEnvelope(originalEvent.TenantId, originalEvent.PrincipalId, source, aggregateId, payload);
        IInitializableCommandEnvelope initializable = (IInitializableCommandEnvelope)commandEnvelope;
        initializable.ParentCommandId = originalEvent.CommandId;

        return _innerBus.Send<TId, TPayload>(commandEnvelope, cancellationToken);
    }
}