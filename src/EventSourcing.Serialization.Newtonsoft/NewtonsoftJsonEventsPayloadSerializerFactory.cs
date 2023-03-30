using EventSourcing.Abstractions.Contracts;

namespace EventSourcing.Serialization.Newtonsoft;

public sealed class NewtonsoftJsonEventsPayloadSerializerFactory : IEventsPayloadSerializerFactory
{
    private readonly IPayloadSerializer _serializer;
    
    public NewtonsoftJsonEventsPayloadSerializerFactory(IEventTypeMappingHandler mappingHandler)
    {
        _serializer = new NewtonsoftJsonPayloadSerializer(mappingHandler);
    }
    
    public IPayloadSerializer GetSerializer()
    {
        return _serializer;
    }
}