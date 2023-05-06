using System;
using EventSourcing.Net.Abstractions.Identities;

namespace EventSourcing.Net.Abstractions.Contracts;

/// <summary>
/// Envelope for the command.
/// </summary>
public interface ICommandEnvelope
{
    /// <summary>
    /// Tenant id.
    /// </summary>
    TenantId TenantId { get; }
        
    /// <summary>
    /// Command sequence id.
    /// </summary>
    CommandSequenceId SequenceId { get; }
        
    /// <summary>
    /// Command id.
    /// </summary>
    CommandId CommandId { get; }
        
    /// <summary>
    /// Parent command id.
    /// </summary>
    CommandId ParentCommandId { get; }
        
    /// <summary>
    /// Timestamp.
    /// </summary>
    DateTime Timestamp { get; }
        
    /// <summary>
    /// Aggregate id.
    /// </summary>
    object AggregateId { get; }
        
    /// <summary>
    /// Command data.
    /// </summary>
    ICommand Payload { get; }

    /// <summary>
    /// Principal id.
    /// </summary>
    PrincipalId PrincipalId { get; }
        
    /// <summary>
    /// Command source.
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

/// <inheritdoc />
public record CommandEnvelope<TId, TPayload> : ICommandEnvelope<TId, TPayload> where TPayload : ICommand 
{
    public TenantId TenantId { get; init; }
    public CommandSequenceId SequenceId { get; init; }
    public CommandId CommandId { get; init; }
    public CommandId ParentCommandId { get; init; }
    public DateTime Timestamp { get; init; }
    public TId AggregateId { get; init; }
    public TPayload Payload { get; init; }
    public PrincipalId PrincipalId { get; init; }
    public string Source { get; init; }
}