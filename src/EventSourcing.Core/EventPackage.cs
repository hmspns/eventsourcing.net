using System;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Abstractions.Types;

namespace EventSourcing.Core;

/// <inheritdoc />
public record EventPackage : IEventPackage
{
    public TenantId TenantId { get; set; }
    public EventId EventId { get; set; }
    public StreamId StreamName { get; set; }
    public object? Payload { get; set; }
    public DateTime Timestamp { get; set; }
    public CommandSequenceId SequenceId { get; set; }
    public CommandId CommandId { get; set; }
    public AggregateVersion StreamPosition { get; set; }
    public PrincipalId PrincipalId { get; set; }
}