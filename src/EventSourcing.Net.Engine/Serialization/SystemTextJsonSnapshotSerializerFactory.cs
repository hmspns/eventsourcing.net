using System.Text.Json;
using System.Text.Json.Serialization;
using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Net.Engine.Serialization;

/// <inheritdoc />
public sealed class SystemTextJsonSnapshotSerializerFactory : ISnapshotSerializerFactory
{
    private readonly SystemTextJsonPayloadSerializer _serializer;
    
    public SystemTextJsonSnapshotSerializerFactory(JsonSerializerOptions? options = null)
    {
        if (options == null)
        {
            options = new JsonSerializerOptions()
            {
                WriteIndented = false,
                Converters =
                {
                    new IIdentityConverter(),
                    new JsonStringEnumConverter()
                }
            };
        }
        else
        {
            options.Converters.Add(new IIdentityConverter());
        }
        
        _serializer = new SystemTextJsonPayloadSerializer(options);
    }
    
    public IPayloadSerializer Get()
    {
        return _serializer;
    }
}