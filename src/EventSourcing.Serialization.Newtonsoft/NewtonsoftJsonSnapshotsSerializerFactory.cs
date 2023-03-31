using EventSourcing.Abstractions.Contracts;

namespace EventSourcing.Serialization.Newtonsoft;

/// <inheritdoc />
public sealed class NewtonsoftJsonSnapshotsSerializerFactory : ISnapshotsSerializerFactory
{
    private readonly NewtonsoftJsonSnapshotPayloadSerializer _serializer = new();
    
    public IPayloadSerializer Get()
    {
        return _serializer;
    }
}