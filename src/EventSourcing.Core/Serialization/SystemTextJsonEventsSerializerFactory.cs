using EventSourcing.Abstractions.Contracts;

namespace EventSourcing.Core.Serialization;

/// <inheritdoc />
public sealed class SystemTextJsonEventsSerializerFactory : IEventsPayloadSerializerFactory
{
    private readonly SystemTextJsonEventsPayloadSerializer _serializer;

    public SystemTextJsonEventsSerializerFactory(IEventTypeMappingHandler handler)
    {
        _serializer = new SystemTextJsonEventsPayloadSerializer(handler);
    }
    
    public IPayloadSerializer GetSerializer()
    {
        return _serializer;
    }
}