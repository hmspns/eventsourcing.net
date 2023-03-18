using System;
using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Core.Contracts
{
    /// <summary>
    /// Aggregate state snapshot.
    /// </summary>
    public interface ISnapshot
    {
        /// <summary>
        /// Does snapshot exists.
        /// </summary>
        bool HasSnapshot { get; }

        /// <summary>
        /// Version of aggregate that snapshot belongs to.
        /// </summary>
        AggregateVersion Version { get; }

        /// <summary>
        /// State data.
        /// </summary>
        object State { get; }

        /// <summary>
        /// Id of snapshot.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Name of the stream.
        /// </summary>
        StreamId StreamName { get; }
    }
}