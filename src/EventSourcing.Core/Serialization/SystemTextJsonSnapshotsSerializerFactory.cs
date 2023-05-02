using System.Text.Json;
using System.Text.Json.Serialization;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Core.Serialization;

/// <inheritdoc />
public sealed class SystemTextJsonSnapshotsSerializerFactory : ISnapshotsSerializerFactory
{
    private readonly SystemTextJsonPayloadSerializer _serializer;
    
    public SystemTextJsonSnapshotsSerializerFactory(JsonSerializerOptions? options = null)
    {
        options ??= new JsonSerializerOptions()
        {
            WriteIndented = false,
            Converters =
            {
                new IIdentityConverter(),
                new JsonStringEnumConverter()
            }
        };

        _serializer = new SystemTextJsonPayloadSerializer(options);
    }
    
    public IPayloadSerializer Get()
    {
        return _serializer;
    }
}