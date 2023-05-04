using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Abstractions.Types;

namespace EventSourcing.Net.Abstractions.Contracts;

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
    object? State { get; }

    /// <summary>
    /// Name of the stream.
    /// </summary>
    StreamId StreamName { get; }
}