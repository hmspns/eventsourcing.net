﻿using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Abstractions.Types;

namespace EventSourcing.Net.Engine;

/// <inheritdoc />
public record Snapshot : ISnapshot
{
    public Snapshot()
    {
        Version = AggregateVersion.NotCreated;
        State = null;
    }

    public Snapshot(StreamId streamId, object? state, AggregateVersion version)
    {
        StreamName = streamId;
        State = state;
        Version = version;
    }

    public static Snapshot Empty(StreamId streamId)
    {
        return new Snapshot(streamId, null, AggregateVersion.NotCreated);
    }

    public bool HasSnapshot => State != null;

    public AggregateVersion Version { get; init; }

    public object? State { get; init; }

    public StreamId StreamName { get; init; }
}