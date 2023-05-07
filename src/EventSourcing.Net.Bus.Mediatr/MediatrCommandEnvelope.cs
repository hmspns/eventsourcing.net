using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Engine;
using MediatR;

namespace EventSourcing.Net.Bus.Mediatr;

/// <inheritdoc />
public class MediatrCommandEnvelope<TId, TPayload> : 
    ICommandEnvelope<TId, TPayload>,
    IRequest<ICommandExecutionResult<TId>>
    where TPayload : ICommand
{
    public TenantId TenantId { get; init; }
    public CommandSequenceId SequenceId { get; init; }
    public CommandId CommandId { get; init; }
    public CommandId ParentCommandId { get; init; }
    public DateTime Timestamp { get; init; }
    public TPayload Payload { get; init; }
    public TId AggregateId { get; init; }
    public PrincipalId PrincipalId { get; init; }
    public string Source { get; init; }
}