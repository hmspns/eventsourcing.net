namespace EventSourcing.Net.Abstractions.Types;

using System;
using Contracts;
using Identities;

/// <inheritdoc />
public record CommandEnvelope<TId, TPayload> :
    IInitializableCommandEnvelope,
    ICommandEnvelope<TId, TPayload> where TPayload : ICommand 
{
    public TenantId TenantId { get; set; }
    public CommandSequenceId SequenceId { get; set; }
    public CommandId CommandId { get; set; }
    public CommandId ParentCommandId { get; set; }
    public DateTime Timestamp { get; set; }
    public TId AggregateId { get; set; }
    public TPayload Payload { get; set; }
    public PrincipalId PrincipalId { get; set; }
    public string Source { get; set; }
    
    object IInitializableCommandEnvelope.Payload
    {
        set => Payload = (TPayload)value;
    }
    
    object IInitializableCommandEnvelope.AggregateId
    {
        set => AggregateId = (TId)value;
    }
}