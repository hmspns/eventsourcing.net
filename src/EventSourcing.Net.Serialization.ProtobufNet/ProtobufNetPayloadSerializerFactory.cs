using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Net.Serialization.ProtobufNet;

/// <inheritdoc />
public sealed class ProtobufNetPayloadSerializerFactory : IPayloadSerializerFactory
{
    private readonly ProtobufNetPayloadSerializer _protobufNetPayloadSerializer = new ProtobufNetPayloadSerializer();

    public IPayloadSerializer GetSerializer()
    {
        return _protobufNetPayloadSerializer;
    }
}

/// <inheritdoc />
public sealed class ProtobufNetSnapshotSerializerFactory : ISnapshotSerializerFactory
{
    private readonly ProtobufNetPayloadSerializer _protobufNetPayloadSerializer = new ProtobufNetPayloadSerializer();

    public IPayloadSerializer GetSerializer()
    {
        return _protobufNetPayloadSerializer;
    }
}