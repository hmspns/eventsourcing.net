using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Net.Serialization.ProtobufNet;

/// <summary>
/// Provide extensions for registration.
/// </summary>
public static class ProtobufNetEventSourcingSerializationOptionsExtensions
{
    /// <summary>
    /// Configure EventSourcing.Net to use System.Text.Json serialization.
    /// </summary>
    /// <param name="options">Serialization options.</param>
    /// <param name="configurator">Callback to configure serialization.</param>
    public static void UseProtobufNet(this EventSourcingSerializationOptions options)
    {
        options.ReplaceSingleton<IPayloadSerializerFactory, ProtobufNetPayloadSerializerFactory>();
        options.ReplaceSingleton<ISnapshotSerializerFactory, ProtobufNetSnapshotSerializerFactory>();
    }
}