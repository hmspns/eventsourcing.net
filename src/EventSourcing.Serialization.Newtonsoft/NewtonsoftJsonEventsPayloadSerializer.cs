using System.Buffers;
using System.Text;
using EventSourcing.Abstractions;
using EventSourcing.Abstractions.Contracts;
using Newtonsoft.Json;

namespace EventSourcing.Serialization.Newtonsoft;

/// <inheritdoc />
public sealed class NewtonsoftJsonEventsPayloadSerializer : IPayloadSerializer
{
    private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings()
    {
        Formatting = Formatting.Indented,
        ReferenceLoopHandling = ReferenceLoopHandling.Error,
        Converters = new List<JsonConverter>()
        {
            new IdentityConverter()
        }
    };

    private readonly IEventTypeMappingHandler _eventTypeMappingHandler;

    public NewtonsoftJsonEventsPayloadSerializer(IEventTypeMappingHandler eventTypeMappingHandler)
    {
        _eventTypeMappingHandler = eventTypeMappingHandler;
    }
    
    public byte[] Serialize(object obj, out string type)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        type = _eventTypeMappingHandler.GetStringRepresentation(obj.GetType());
        return SerializeJson(obj);
    }

    public object Deserialize(Memory<byte> data, string type)
    {
        if (data.IsEmpty)
        {
            throw new ArgumentException("Data is empty", nameof(data));
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Type must be not an empty string", nameof(type));
        }

        Type t = _eventTypeMappingHandler.GetEventType(type);
        
        return DeserializeJson(t, data);
    }

    private byte[] SerializeJson(object obj)
    {
        using MemoryStream ms = new MemoryStream();
        using TextWriter tw = new StreamWriter(ms);
        using JsonTextWriter jw = new JsonTextWriter(tw)
        {
            ArrayPool = JsonArrayPool.Instance
        };
        JsonSerializer serializer = JsonSerializer.CreateDefault(_settings);
        serializer.Serialize(jw, obj);
        return ms.ToArray();
    }
    
    private object DeserializeJson(Type objectType, Memory<byte> content)
    {
        using MemoryStream ms = new MemoryStream(content.ToArray());
        using TextReader tr = new StreamReader(ms);
        using JsonTextReader jr = new JsonTextReader(tr)
        {
            ArrayPool = JsonArrayPool.Instance
        };
        JsonSerializer serializer = JsonSerializer.CreateDefault(_settings);
        return serializer.Deserialize(jr, objectType);
    }
}

internal sealed class JsonArrayPool : IArrayPool<char>
{
    private static readonly JsonArrayPool _instance = new JsonArrayPool();

    internal static JsonArrayPool Instance => _instance;
        
    public char[] Rent(int minimumLength)
    {
        return ArrayPool<char>.Shared.Rent(minimumLength);
    }

    public void Return(char[]? array)
    {
        if (array != null)
        {
            ArrayPool<char>.Shared.Return(array);
        }
    }
}