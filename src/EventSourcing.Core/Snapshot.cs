using System;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Abstractions.Types;

namespace EventSourcing.Core
{
    /// <inheritdoc />
    public record Snapshot : ISnapshot
    {
        public Snapshot()
        {
            HasSnapshot = false;
            Version = AggregateVersion.NotCreated;
            State = null;
        }

        public Snapshot(StreamId streamId, object state, AggregateVersion version)
        {
            StreamName = streamId;
            State = state;
            HasSnapshot = state != null;
            Version = version;
        }

        public static Snapshot Empty(StreamId streamId)
        {
            return new Snapshot(streamId, null, AggregateVersion.NotCreated);
        }

        public bool HasSnapshot { get; init; }

        public AggregateVersion Version { get; init; }

        public object State { get; init; }

        public StreamId StreamName { get; init; }
    }
}