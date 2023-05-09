using System;

namespace EventSourcing.Net.Abstractions.Contracts;

/// <summary>
/// Factory to create snapshot serializer.
/// </summary>
public interface ISnapshotSerializerFactory
{
    /// <summary>
    /// Return payload serializer.
    /// </summary>
    /// <returns>Payload serializer.</returns>
    IPayloadSerializer GetSerializer();

    /// <summary>
    /// Return payload serializer.
    /// </summary>
    /// <returns>Payload serializer.</returns>
    [Obsolete("Use GetSerializer instead")]
    IPayloadSerializer Get() => GetSerializer();
}