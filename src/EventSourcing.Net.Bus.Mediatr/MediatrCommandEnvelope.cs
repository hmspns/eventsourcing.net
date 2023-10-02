using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Engine;
using MediatR;

namespace EventSourcing.Net.Bus.Mediatr;

/// <inheritdoc />
public class MediatrCommandEnvelope<TId, TPayload> : 
    ICommandEnvelope<TId, TPayload>,
    IInitializableCommandEnvelope,
    IRequest<ICommandExecutionResult<TId>>
    where TPayload : ICommand
{
    public TenantId TenantId { get; set; }
    public CommandSequenceId SequenceId { get; set; }
    public CommandId CommandId { get; set; }
    public CommandId ParentCommandId { get; set; }
    public DateTime Timestamp { get; set; }
    public TPayload Payload { get; set; }
    public TId AggregateId { get; set; }
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