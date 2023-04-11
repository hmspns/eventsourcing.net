using EventSourcing.Abstractions.Contracts;

namespace EventSourcing.Serialization.Newtonsoft;

/// <inheritdoc />
public sealed class NewtonsoftJsonSnapshotsSerializerFactory : ISnapshotsSerializerFactory
{
    public IPayloadSerializer Get()
    {
        return new NewtonsoftJsonPayloadSerializer();
    }
}