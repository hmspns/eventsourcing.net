using EventSourcing.Abstractions.Contracts;

namespace EventSourcing.Core.Serialization;

/// <inheritdoc />
public sealed class SystemTextJsonSnapshotsSerializerFactory : ISnapshotsSerializerFactory
{
    private readonly SystemTextJsonSnapshotPayloadSerializer _serializer;
    
    public SystemTextJsonSnapshotsSerializerFactory()
    {
        _serializer = new SystemTextJsonSnapshotPayloadSerializer();
    }
    
    public IPayloadSerializer Get()
    {
        return _serializer;
    }
}