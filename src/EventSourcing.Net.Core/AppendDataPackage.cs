using System;
using System.Collections.Generic;
using System.Linq;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;

namespace EventSourcing.Net.Core;

/// <inheritdoc />
public record AppendDataPackage<TId> : IAppendDataPackage<TId>
{
    public AppendDataPackage(IAppendCommandPackage<TId> command, params IAppendEventPackage[] events)
    {
        CommandPackage = command;
        EventPackages = events;
    }
        
    public AppendDataPackage(IAppendCommandPackage<TId> command, IEnumerable<IAppendEventPackage> events)
    {
        CommandPackage = command;
        EventPackages = events.ToArray();
    }

    public IAppendCommandPackage<TId> CommandPackage { get; }
        
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
public record AppendCommandPackage<TId> : IAppendCommandPackage<TId>
{
    public TenantId TenantId { get; init; }
    public CommandId CommandId { get; init; }
    public CommandId ParentCommandId { get; init; }
    public CommandSequenceId SequenceId { get; init; }
    public DateTime Timestamp { get; init; }
    public TId AggregateId { get; init; }
    public PrincipalId PrincipalId { get; init; }
    public string Source { get; init; }
    public object Payload { get; init; }
}