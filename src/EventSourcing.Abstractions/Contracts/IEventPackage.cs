using System;
using System.Collections.Generic;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Abstractions.Types;

namespace EventSourcing.Abstractions.Contracts;

/// <summary>
/// Data of events from storage.
/// </summary>
public interface IEventsData
{
    /// <summary>
    /// Events.
    /// </summary>
    public IReadOnlyCollection<IEventPackage> Events { get; }
        
    /// <summary>
    /// Stream end position.
    /// </summary>
    public StreamPosition StreamEndPosition { get; }
}
    
/// <summary>
/// Package with information about specific event.
/// </summary>
public interface IEventPackage
{
    /// <summary>
    /// Id of tenant.
    /// </summary>
    public TenantId TenantId { get; set; }
        
    /// <summary>
    /// Id of event.
    /// </summary>
    public EventId EventId { get; set; }
        
    /// <summary>
    /// Name of stream.
    /// </summary>
    public StreamId StreamName { get; set; }

    /// <summary>
    /// Event data.
    /// </summary>
    public object? Payload { get; set; }
        
    /// <summary>
    /// Timestamp of event.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Id of sequence.
    /// </summary>
    public CommandSequenceId SequenceId { get; set; }
        
    /// <summary>
    /// Id of command.
    /// </summary>
    public CommandId CommandId { get; set; }

    /// <summary>
    /// Version of aggregate.
    /// </summary>
    public AggregateVersion StreamPosition { get; set; }
        
    /// <summary>
    /// Id of principal.
    /// </summary>
    public PrincipalId PrincipalId { get; set; }
}