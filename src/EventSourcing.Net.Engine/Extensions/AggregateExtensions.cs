namespace EventSourcing.Net.Engine.Extensions;

using System.Runtime.CompilerServices;
using Abstractions.Contracts;

/// <summary>
/// Extensions for IAggregate&lt;TId&gt;.
/// </summary>
public static class AggregateExtensions
{
    /// <summary>
    /// Return status depends on committed events.
    /// </summary>
    /// <param name="aggregate">Aggregate.</param>
    /// <param name="commandEnvelope">Command data.</param>
    /// <returns>Return Accepted and HasChanges if there is any event that should be committed. Otherwise return NoChange.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CommandExecutionResult<TId> OkIfChanges<TId>(this IAggregate<TId> aggregate, ICommandEnvelope<TId> commandEnvelope)
    {
        return CommandExecutionResult<TId>.OkIfChanges(aggregate, commandEnvelope);
    }

    /// <summary>
    /// Return for accepted command that made changes.
    /// </summary>
    /// <param name="aggregate">Aggregate.</param>
    /// <param name="commandEnvelope">Command data.</param>
    /// <returns>Status of the executed command.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CommandExecutionResult<TId> Ok<TId>(this IAggregate<TId> aggregate, ICommandEnvelope<TId> commandEnvelope)
    {
        return CommandExecutionResult<TId>.Ok(commandEnvelope);
    }

    /// <summary>
    /// Status for accepted command but without any changes.
    /// </summary>
    /// <param name="aggregate">Aggregate.</param>
    /// <param name="commandEnvelope">Command data.</param>
    /// <returns>Status of the executed command.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CommandExecutionResult<TId> NoChange<TId>(this IAggregate<TId> aggregate, ICommandEnvelope<TId> commandEnvelope)
    {
        return CommandExecutionResult<TId>.NoChange(commandEnvelope);
    }

    /// <summary>
    /// Command not valid.
    /// </summary>
    /// <param name="aggregate">Aggregate.</param>
    /// <param name="commandEnvelope">Command data.</param>
    /// <returns>Status of the executed command.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CommandExecutionResult<TId> CommandNotValid<TId>(this IAggregate<TId> aggregate, ICommandEnvelope<TId> commandEnvelope)
    {
        return CommandExecutionResult<TId>.CommandNotValid(commandEnvelope);
    }

    /// <summary>
    /// Something went wrong during aggregate execution.
    /// </summary>
    /// <param name="aggregate">Aggregate.</param>
    /// <param name="commandEnvelope">Command data.</param>
    /// <param name="message">Error message.</param>
    /// <returns>Status of the executed command.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CommandExecutionResult<TId> Error<TId>(this IAggregate<TId> aggregate, ICommandEnvelope<TId> commandEnvelope, string? message)
    {
        return CommandExecutionResult<TId>.Error(commandEnvelope, message);
    }

    /// <summary>
    /// Trying to modify entity that not exists.
    /// </summary>
    /// <param name="aggregate">Aggregate.</param>
    /// <param name="commandEnvelope">Command data.</param>
    /// <returns>Status of the executed command.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CommandExecutionResult<TId> NotExists<TId>(this IAggregate<TId> aggregate, ICommandEnvelope<TId> commandEnvelope, string? message = null)
    {
        return CommandExecutionResult<TId>.NotExists(commandEnvelope, message);
    }
}