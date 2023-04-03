using System;
using System.Linq;
using System.Runtime.CompilerServices;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Core;

public record CommandExecutionResult<TId> : ICommandExecutionResult<TId>
{
    /// <summary>
    /// Return status depends on committed events.
    /// </summary>
    /// <param name="aggregate">Aggregate.</param>
    /// <param name="commandEnvelope">Command data.</param>
    /// <returns>Return Accepted and HasChanges if there is any event that should be committed. Otherwise return NoChange.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CommandExecutionResult<TId> OkIfChanges(IAggregate<TId> aggregate, ICommandEnvelope<TId> commandEnvelope)
    {
        if (aggregate.Uncommitted.Any())
        {
            return new CommandExecutionResult<TId>(commandEnvelope, true, true, null);
        }

        return NoChange(commandEnvelope);
    }

    /// <summary>
    /// Status for accepted command but without any changes.
    /// </summary>
    /// <param name="commandEnvelope">Command data.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CommandExecutionResult<TId> NoChange(ICommandEnvelope<TId> commandEnvelope)
    {
        return new CommandExecutionResult<TId>(commandEnvelope, true, false, null);
    }

    /// <summary>
    /// Command not valid.
    /// </summary>
    /// <param name="commandEnvelope">Command data.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CommandExecutionResult<TId> CommandNotValid(ICommandEnvelope<TId> commandEnvelope)
    {
        return new CommandExecutionResult<TId>(commandEnvelope, false, false, "Command not valid");
    }

    /// <summary>
    /// Something went wrong during aggregate execution.
    /// </summary>
    /// <param name="commandEnvelope">Command data.</param>
    /// <param name="message">Error message.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CommandExecutionResult<TId> Error(ICommandEnvelope<TId> commandEnvelope, string? message)
    {
        return new CommandExecutionResult<TId>(commandEnvelope, false, false, message);
    }

    /// <summary>
    /// Trying to modify entity that not exists.
    /// </summary>
    /// <param name="commandEnvelope">Command data.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CommandExecutionResult<TId> NotExists(ICommandEnvelope<TId> commandEnvelope, string? message = null)
    {
        if (message == null)
        {
            message = "Aggregate not exists";
        }
        return new CommandExecutionResult<TId>(commandEnvelope, false, false, message);
    }
        
    /// <summary>
    /// Initialize new object.
    /// </summary>
    /// <param name="commandEnvelope">Command data.</param>
    /// <param name="isAccepted">Whether command was accepted.</param>
    /// <param name="hasChanges">Whether there are some changes (= events were produced).</param>
    /// <param name="errorCode">Optional error code.</param>
    public CommandExecutionResult(ICommandEnvelope<TId> commandEnvelope, bool isAccepted, bool hasChanges, string? errorCode = null)
    {
        AggregateId = commandEnvelope.AggregateId;
        CommandId = commandEnvelope.CommandId;
        CommandSequenceId = commandEnvelope.SequenceId;
        IsAccepted = isAccepted;
        HasChanges = hasChanges;
        ErrorCode = errorCode;
        CommandType = commandEnvelope.Payload.GetType();
    }
        
    /// <summary>
    /// Aggregate id.
    /// </summary>
    public TId AggregateId { get; }

    /// <summary>
    /// Command id.
    /// </summary>
    public CommandId CommandId { get; }

    /// <summary>
    /// Id of commands sequence.
    /// </summary>
    public CommandSequenceId CommandSequenceId { get; }
        
    /// <summary>
    /// Whether command accepted by aggregate.
    /// </summary>
    public bool IsAccepted { get; }

    /// <summary>
    /// Whether there are any changes (means events were generated).
    /// </summary>
    public bool HasChanges { get; }

    /// <summary>
    /// Error code.
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// Command type.
    /// </summary>
    public Type CommandType { get; }
}