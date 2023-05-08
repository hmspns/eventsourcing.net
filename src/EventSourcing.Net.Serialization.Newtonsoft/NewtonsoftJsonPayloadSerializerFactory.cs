using EventSourcing.Net.Abstractions.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EventSourcing.Net.Serialization.Newtonsoft;

/// <summary>
/// Implementation of payload serializer factory based on Newtonsoft.Json.
/// </summary>
public sealed class NewtonsoftJsonPayloadSerializerFactory : IPayloadSerializerFactory
{
    private readonly NewtonsoftJsonPayloadSerializer _serializer;
    
    public NewtonsoftJsonPayloadSerializerFactory(JsonSerializerSettings? settings = null)
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

    /// <summary>
    /// Return payload serializer.
    /// </summary>
    /// <returns>Payload serializer.</returns>
    public IPayloadSerializer GetSerializer()
    {
        return _serializer;
    }
}