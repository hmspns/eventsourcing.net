using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Serialization.Newtonsoft;

/// <inheritdoc />
public sealed class NewtonsoftJsonSnapshotsSerializerFactory : ISnapshotsSerializerFactory
{
    public IPayloadSerializer Get()
    {
        return new NewtonsoftJsonPayloadSerializer();
    }
}