using System;
using EventSourcing.Net.Abstractions.Identities;

namespace EventSourcing.Net.Abstractions.Contracts;

/// <summary>
/// Envelope for the command.
/// </summary>
public interface ICommandEnvelope
{
    /// <summary>
    /// Get id of the tenant.
    /// </summary>
    TenantId TenantId { get; }
        
    /// <summary>
    /// Get id of the command sequence.
    /// </summary>
    CommandSequenceId SequenceId { get; }
        
    /// <summary>
    /// Get id of the command.
    /// </summary>
    CommandId CommandId { get; }
        
    /// <summary>
    /// Get id of the parent command.
    /// </summary>
    CommandId ParentCommandId { get; }
        
    /// <summary>
    /// Get timestamp.
    /// </summary>
    DateTime Timestamp { get; }
        
    /// <summary>
    /// Get id of the aggregate.
    /// </summary>
    object AggregateId { get; }
        
    /// <summary>
    /// Get command data.
    /// </summary>
    ICommand Payload { get; }

    /// <summary>
    /// Get id of the principal.
    /// </summary>
    PrincipalId PrincipalId { get; }
        
    /// <summary>
    /// Get the command source.
    /// </summary>
    string Source { get; }
}

/// <inheritdoc />
public interface ICommandEnvelope<out TId> : ICommandEnvelope
{
    new TId AggregateId { get; }
}

/// <inheritdoc />
public interface ICommandEnvelope<out TId, out TPayload> : ICommandEnvelope<TId> where TPayload : ICommand
{
    new TPayload Payload { get; }
        
    object ICommandEnvelope.AggregateId => AggregateId;

    ICommand ICommandEnvelope.Payload => Payload;
}