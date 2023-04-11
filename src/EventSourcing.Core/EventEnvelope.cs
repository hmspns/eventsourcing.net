using System;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Abstractions.Types;

namespace EventSourcing.Core;

/// <summary>
/// Interface to encapsulate set methods.
/// </summary>
internal interface IInitializablePayloadEvent<in TId>
{
    EventId EventId { set; }
        
    TId AggregateId { set; }
        
    object Payload { set; }
        
    DateTime Timestamp { set; }
        
    CommandId CommandId { set; }
        
    CommandSequenceId SequenceId { set; }
        
    AggregateVersion Version { set; }
        
    PrincipalId PrincipalId { set; }
        
    TenantId TenantId { set; }
}

/// <inheritdoc />
public class EventEnvelope<TId, TPayload> : IEventEnvelope<TId, TPayload>, IInitializablePayloadEvent<TId> where TPayload : IEvent
{
    public EventId EventId { get; set; } = EventId.New();
        
    public TId AggregateId { get; set; }

    public TPayload Payload { get; set; }

    public DateTime Timestamp { get; set; }

    public CommandId CommandId { get; set; }
        
    public CommandSequenceId SequenceId { get; set; }
        
    public AggregateVersion Version { get; set; }
        
    public PrincipalId PrincipalId { get; set; }
        
    public TenantId TenantId { get; set; }

    object IInitializablePayloadEvent<TId>.Payload
    {
        set => Payload = (TPayload)value;
    }
}