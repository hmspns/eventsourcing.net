using System.Collections.Generic;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Abstractions.Types;

namespace EventSourcing.Abstractions.Contracts
{
    /// <summary>
    /// The typed version of this interface must implement each aggregate.
    /// </summary>
    public interface IAggregate
    {
        /// <summary>
        /// Events that were generated but not committed yet.
        /// </summary>
        internal IReadOnlyList<IEventEnvelope> Uncommitted { get; }
        
        /// <summary>
        /// Aggregate id.
        /// </summary>
        object AggregateId { get; }
        
        /// <summary>
        /// Aggregate version.
        /// </summary>
        AggregateVersion Version { get; }

        /// <summary>
        /// Name of the stream.
        /// </summary>
        StreamId StreamName { get; }
        
        /// <summary>
        /// Load snapshot into the aggregate.
        /// </summary>
        /// <param name="snapshot">Snapshot.</param>
        internal void LoadSnapshot(ISnapshot snapshot);

        /// <summary>
        /// Load events into the aggregate.
        /// </summary>
        /// <param name="eventsStream">Stream of events.</param>
        internal void LoadEvents(EventsStream eventsStream);

        /// <summary>
        /// Marked the aggregate as committed.
        /// </summary>
        /// <param name="appendResult">Result of save events process.</param>
        /// <returns>Snapshot of the aggregate.</returns>
        internal ISnapshot Commit(IAppendEventsResult appendResult);
    }

    /// <summary>
    /// Typed version of <see cref="IAggregate"/> interface.
    /// </summary>
    /// <typeparam name="TId">Type of aggregate id.</typeparam>
    public interface IAggregate<out TId> : IAggregate
    {
        /// <summary>
        /// Aggregate id.
        /// </summary>
        new TId AggregateId { get; }

        /// <summary>
        /// Aggregate id.
        /// </summary>
        object IAggregate.AggregateId => AggregateId;
    }
}