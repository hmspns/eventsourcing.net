using EventSourcing.Abstractions.Contracts;

namespace EventSourcing.Serialization.Newtonsoft;

public sealed class NewtonsoftJsonEventsPayloadSerializerFactory : IEventsPayloadSerializerFactory
{
    public IPayloadSerializer GetSerializer()
    {
        return new NewtonsoftJsonPayloadSerializer();
    }
}