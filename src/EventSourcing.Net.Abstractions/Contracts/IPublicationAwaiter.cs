using System;
using System.Threading.Tasks;
using EventSourcing.Net.Abstractions.Identities;

namespace EventSourcing.Net.Abstractions.Contracts;

/// <summary>
/// Provide functionality to await moment when event will be published.
/// </summary>
public interface IPublicationAwaiter
{
    /// <summary>
    /// Mark publication as ready.
    /// </summary>
    /// <param name="sequenceId">Id of sequence of commands.</param>
    public void MarkAsReady(CommandSequenceId sequenceId);
        
    /// <summary>
    /// Return task to await publication.
    /// </summary>
    /// <param name="sequenceId">Id of sequence of commands.</param>
    /// <param name="timeout">Optional timeout.</param>
    Task WaitForPublication(CommandSequenceId sequenceId, TimeSpan timeout = default);
}