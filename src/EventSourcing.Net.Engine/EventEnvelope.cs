using System;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Abstractions.Types;

namespace EventSourcing.Net.Engine;

internal interface IInitializableEventEnvelope
{
    EventId EventId { set; }
        
    object AggregateId { set; }
        
    object Payload { set; }
        
    DateTime Timestamp { set; }
        
    CommandId CommandId { set; }
        
    CommandSequenceId SequenceId { set; }
        
    AggregateVersion Version { set; }
        
    PrincipalId PrincipalId { set; }
        
    TenantId TenantId { set; }
}

/// <summary>
/// Interface to encapsulate set methods.
/// </summary>
internal interface IInitializableEventEnvelope<in TId> : IInitializableEventEnvelope
{
    TId AggregateId { set; }
}

/// <inheritdoc />
public record EventEnvelope<TId, TPayload> : IEventEnvelope<TId, TPayload>, IInitializableEventEnvelope<TId> where TPayload : IEvent
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

    object IInitializableEventEnvelope.Payload
    {
        set => Payload = (TPayload)value;
    }
    
    object IInitializableEventEnvelope.AggregateId
    {
        set => AggregateId = (TId)value;
    }
}