using System;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Core.Exceptions;

/// <summary>
/// Exception during command execution.
/// </summary>
public sealed class CommandExecutionException : Exception
{
    /// <summary>
    /// Initialize new object.
    /// </summary>
    /// <param name="message">Message.</param>
    /// <param name="commandEnvelope">Command data.</param>
    internal CommandExecutionException(string message, ICommandEnvelope commandEnvelope) : base(message)
    {
        CommandEnvelope = commandEnvelope;
    }
        
    /// <summary>
    /// Get command data.
    /// </summary>
    public ICommandEnvelope CommandEnvelope { get; }
}