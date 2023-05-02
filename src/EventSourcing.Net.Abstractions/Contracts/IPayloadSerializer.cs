using System;

namespace EventSourcing.Net.Abstractions.Contracts;

/// <summary>
/// Contract for payload serialization.
/// </summary>
public interface IPayloadSerializer
{
    /// <summary>
    /// Serialize data.
    /// </summary>
    /// <param name="obj">Data to be serialized.</param>
    /// <returns>Serialized data.</returns>
    byte[] Serialize(object obj);

    /// <summary>
    /// Deserialize data.
    /// </summary>
    /// <param name="type">Type of serialized data.</param>
    /// <param name="data">Binary data.</param>
    /// <returns>Deserialized object.</returns>
    object Deserialize(Type type, Memory<byte> data);
}