namespace EventSourcing.Net.Abstractions.Types;

using System;
using System.Runtime.InteropServices;
using Identities;

/// <summary>
/// Package with information about specific event.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public record struct EventPackage
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