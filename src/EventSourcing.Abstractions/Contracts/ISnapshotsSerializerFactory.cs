namespace EventSourcing.Abstractions.Contracts;

/// <summary>
/// Factory to create snapshot serializer.
/// </summary>
public interface ISnapshotsSerializerFactory
{
    /// <summary>
    /// Return payload serializer.
    /// </summary>
    /// <returns>Payload serializer.</returns>
    IPayloadSerializer Get();
}