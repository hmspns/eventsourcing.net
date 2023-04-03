using System;
using System.Collections.Generic;
using System.Linq;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Core;

/// <inheritdoc />
public record AppendDataPackage : IAppendDataPackage
{
    public AppendDataPackage(IAppendCommandPackage command, params IAppendEventPackage[] events)
    {
        CommandPackage = command;
        EventPackages = events;
    }
        
    public AppendDataPackage(IAppendCommandPackage command, IEnumerable<IAppendEventPackage> events)
    {
        CommandPackage = command;
        EventPackages = events.ToArray();
    }

    public IAppendCommandPackage CommandPackage { get; }
        
    public IEnumerable<IAppendEventPackage> EventPackages { get; }
}

/// <inheritdoc />
public record AppendEventPackage : IAppendEventPackage
{
    public EventId EventId { get; init; }
    public StreamId StreamName { get; init; }
    public object Payload { get; init; }
    public DateTime Timestamp { get; init; }
}

/// <inheritdoc />
public record AppendCommandPackage : IAppendCommandPackage
{
    public TenantId TenantId { get; init; }
    public CommandId CommandId { get; init; }
    public CommandId ParentCommandId { get; init; }
    public CommandSequenceId SequenceId { get; init; }
    public DateTime Timestamp { get; init; }
    public object AggregateId { get; init; }
    public PrincipalId PrincipalId { get; init; }
    public string Source { get; init; }
    public object Payload { get; init; }
}