using EventSourcing.Abstractions.Contracts;

namespace EventSourcing.Core.Serialization;

/// <inheritdoc />
public sealed class SystemTextJsonEventsSerializerFactory : IEventsPayloadSerializerFactory
{
    private readonly SystemTextJsonPayloadSerializer _serializer;

    public SystemTextJsonEventsSerializerFactory(IEventTypeMappingHandler handler)
    {
        _serializer = new SystemTextJsonPayloadSerializer(handler);
    }
    
    public IPayloadSerializer GetSerializer()
    {
        return _serializer;
    }
}

public sealed class SystemTextJsonSnapshotsSerializerFactory
{
    
}