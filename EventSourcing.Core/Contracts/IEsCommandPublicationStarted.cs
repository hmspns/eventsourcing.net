using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Core.Contracts
{
    /// <summary>
    /// Publication of commands was started.
    /// </summary>
    public interface IEsCommandPublicationStarted
    {
        /// <summary>
        /// Id of command.
        /// </summary>
        CommandId CommandId { get; init; }
        
        /// <summary>
        /// Id of commands sequence.
        /// </summary>
        public CommandSequenceId SequenceId { get; init; }
    }

    /// <summary>
    /// Command was completed.
    /// </summary>
    public interface IEsCommandPublicationCompleted
    {
        /// <summary>
        /// Id of command.
        /// </summary>
        CommandId CommandId { get; init; }
        
        /// <summary>
        /// Id of commands sequence.
        /// </summary>
        CommandSequenceId SequenceId { get; init; }
    }

    /// <summary>
    /// Sequence of commands was completed.
    /// </summary>
    public interface IEsSequenceCompleted
    {
        /// <summary>
        /// Id of commands sequence.
        /// </summary>
        CommandSequenceId SequenceId { get; init; }
    }

    /// <summary>
    /// Notify about start of publication.
    /// </summary>
    /// <param name="CommandId">Id of command.</param>
    /// <param name="SequenceId">If of sequence of commands.</param>
    public record EsCommandPublicationStarted(CommandId CommandId, CommandSequenceId SequenceId) : IEsCommandPublicationStarted;

    /// <summary>
    /// Notify that command is completed.
    /// </summary>
    /// <param name="CommandId">Id of command.</param>
    /// <param name="SequenceId">If of sequence of commands.</param>
    public record EsCommandPublicationCompleted(CommandId CommandId, CommandSequenceId SequenceId) : IEsCommandPublicationCompleted;

    /// <summary>
    /// Notify that sequence is completed.
    /// </summary>
    /// <param name="SequenceId">If of sequence of commands.</param>
    public record EsSequenceCompleted(CommandSequenceId SequenceId) : IEsSequenceCompleted;
}