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
    IPayloadSerializer Get();
}