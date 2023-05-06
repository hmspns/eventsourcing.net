using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Abstractions.Types;
using EventSourcing.Net.Engine;
using MediatR;

namespace EventSourcing.Net.Bus.Mediatr;

public record MediatrEventEnvelope<TId, TPayload> :
    IEventEnvelope<TId, TPayload>,
    IInitializableEventEnvelope,
    INotification
    where TPayload : IEvent
{
    public TenantId TenantId { get; set; }
    public EventId EventId { get; set; }
    public TId AggregateId { get; set; }
    public TPayload Payload { get; set; }
    public DateTime Timestamp { get; set; }
    public CommandId CommandId { get; set; }
    public CommandSequenceId SequenceId { get; set; }
    public AggregateVersion Version { get; set; }
    public PrincipalId PrincipalId { get; set; }
    
    object IInitializableEventEnvelope.Payload
    {
        set => Payload = (TPayload)value;
    }
    
    object IInitializableEventEnvelope.AggregateId
    {
        set => AggregateId = (TId)value;
    }
}