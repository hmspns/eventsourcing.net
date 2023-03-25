using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Abstractions.Contracts
{
    /// <summary>
    /// Result of executed command.
    /// </summary>
    /// <typeparam name="TId">Type of aggregate id.</typeparam>
    public interface ICommandExecutionResult<out TId>
    {
        /// <summary>
        /// Aggregate id.
        /// </summary>
        TId AggregateId { get; }

        /// <summary>
        /// Command id.
        /// </summary>
        CommandId CommandId { get; }

        /// <summary>
        /// Id of commands sequence.
        /// </summary>
        CommandSequenceId CommandSequenceId { get; }

        /// <summary>
        /// Whether command was accepted.
        /// </summary>
        bool IsAccepted { get; }

        /// <summary>
        /// Whether there are any changes (means events were generated).
        /// </summary>
        bool HasChanges { get; }

        /// <summary>
        /// Error code.
        /// </summary>
        string ErrorCode { get; }
    }
}