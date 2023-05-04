using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Net.Serialization.Newtonsoft;

/// <inheritdoc />
public sealed class NewtonsoftJsonSnapshotsSerializerFactory : ISnapshotsSerializerFactory
{
    public IPayloadSerializer Get()
    {
        return new NewtonsoftJsonPayloadSerializer();
    }
}