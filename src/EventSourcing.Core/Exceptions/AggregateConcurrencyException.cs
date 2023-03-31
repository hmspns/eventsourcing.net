using System;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Abstractions.Types;

namespace EventSourcing.Core.Exceptions;

/// <summary>
/// Exception happens when 2 or more instances of aggregate trying to write to the same stream.
/// </summary>
public class AggregateConcurrencyException<TId> : Exception
{
        
    /// <summary>
    /// Initialize new object.
    /// </summary>
    /// <param name="message">Command message.</param>
    /// <param name="innerException">Inner exception.</param>
    public AggregateConcurrencyException(string message, Exception innerException) : base(message, innerException)
    {
            
    }
        
    /// <summary>
    /// Get aggregate id.
    /// </summary>
    public TId AggregateId { get; init; }
        
    /// <summary>
    /// Get command id. 
    /// </summary>
    public CommandId CommandId { get; init; }
        
    /// <summary>
    /// Get command sequence id.
    /// </summary>
    public CommandSequenceId SequenceId { get; init; }
        
    /// <summary>
    /// Expected version of the aggregate.
    /// </summary>
    public AggregateVersion ExpectedVersion { get; init; }
        
    /// <summary>
    /// Actual (version from store) version of aggregate.
    /// </summary>
    public AggregateVersion ActualVersion { get; init; }
}