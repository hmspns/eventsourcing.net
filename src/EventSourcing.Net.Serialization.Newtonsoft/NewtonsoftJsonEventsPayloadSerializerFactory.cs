using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Net.Serialization.Newtonsoft;

public sealed class NewtonsoftJsonEventsPayloadSerializerFactory : IEventsPayloadSerializerFactory
{
    public IPayloadSerializer GetSerializer()
    {
        return new NewtonsoftJsonPayloadSerializer();
    }
}