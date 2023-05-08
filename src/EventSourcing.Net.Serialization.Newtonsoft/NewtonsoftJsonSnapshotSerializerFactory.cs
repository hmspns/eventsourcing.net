using EventSourcing.Net.Abstractions.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EventSourcing.Net.Serialization.Newtonsoft;

/// <inheritdoc />
public sealed class NewtonsoftJsonSnapshotSerializerFactory : ISnapshotSerializerFactory
{
    private readonly NewtonsoftJsonPayloadSerializer _serializer;
    
    public NewtonsoftJsonSnapshotSerializerFactory(JsonSerializerSettings? settings = null)
    {
        if (settings == null)
        {
            settings = new()
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Error,
                Converters = new List<JsonConverter>()
                {
                    new IIdentityConverter(),
                    new StringEnumConverter()
                }
            };
        }
        else
        {
            settings.Converters.Add(new IIdentityConverter());
        }

        _serializer = new NewtonsoftJsonPayloadSerializer(settings);
    }
    
    public IPayloadSerializer Get()
    {
        return _serializer;
    }
}