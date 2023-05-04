using System;
using System.Collections.Generic;
using EventSourcing.Net.Abstractions.Identities;

namespace EventSourcing.Net.Abstractions.Contracts;

/// <summary>
/// Data added to storage.
/// </summary>
public interface IAppendDataPackage<TId>
{
    /// <summary>
    /// Command data.
    /// </summary>
    public IAppendCommandPackage<TId> CommandPackage { get; }
        
    /// <summary>
    /// Events data.
    /// </summary>
    public IEnumerable<IAppendEventPackage> EventPackages { get; }
}

/// <summary>
/// Package with event data.
/// </summary>
public interface IAppendEventPackage
{
    /// <summary>
    /// Event id.
    /// </summary>
    public EventId EventId { get; }
        
    /// <summary>
    /// Name of events stream.
    /// </summary>
    public StreamId StreamName { get; }

    /// <summary>
    /// Event data.
    /// </summary>
    public object Payload { get; }
        
    /// <summary>
    /// Event timestamp.
    /// </summary>
    public DateTime Timestamp { get; }
}

/// <summary>
/// Package with command data.
/// </summary>
public interface IAppendCommandPackage<out TId>
{
    /// <summary>
    /// Tenant id.
    /// </summary>
    public TenantId TenantId { get; }
        
    /// <summary>
    /// Command id.
    /// </summary>
    public CommandId CommandId { get; }
        
    /// <summary>
    /// Parent command id.
    /// </summary>
    public CommandId ParentCommandId { get; }
        
    /// <summary>
    /// Sequence id.
    /// </summary>
    public CommandSequenceId SequenceId { get; }
        
    /// <summary>
    /// Timestamp of command.
    /// </summary>
    public DateTime Timestamp { get; }
        
    /// <summary>
    /// Aggregate id.
    /// </summary>
    public TId AggregateId { get; }
        
    /// <summary>
    /// Principal id.
    /// </summary>
    public PrincipalId PrincipalId { get; }
        
    /// <summary>
    /// Command source.
    /// </summary>
    public string Source { get; }
        
    /// <summary>
    /// Command payload.
    /// </summary>
    public object Payload { get; }
}