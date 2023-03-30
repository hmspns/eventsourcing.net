namespace EventSourcing.Abstractions.Contracts;

/// <summary>
/// Factory to create payload serializer.
/// </summary>
public interface IEventsPayloadSerializerFactory
{
    /// <summary>
    /// Return payload serializer.
    /// </summary>
    /// <returns>Payload serializer.</returns>
    IPayloadSerializer GetSerializer();
}