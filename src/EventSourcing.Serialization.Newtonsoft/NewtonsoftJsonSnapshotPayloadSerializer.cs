using EventSourcing.Abstractions.Contracts;
using Newtonsoft.Json;

namespace EventSourcing.Serialization.Newtonsoft;

public sealed class NewtonsoftJsonSnapshotPayloadSerializer : IPayloadSerializer
{
    private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings()
    {
        Formatting = Formatting.None,
        ReferenceLoopHandling = ReferenceLoopHandling.Error
    };

    public byte[] Serialize(object obj, out string type)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        type = obj.GetType().AssemblyQualifiedName;
        return SerializeJson(obj);
    }

    public object Deserialize(byte[] data, string type)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Type must be not an empty string", nameof(type));
        }

        Type t = Type.GetType(type);
        
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
    
    private object DeserializeJson(Type objectType, byte[] content)
    {
        using MemoryStream ms = new MemoryStream(content);
        using TextReader tr = new StreamReader(ms);
        using JsonTextReader jr = new JsonTextReader(tr)
        {
            ArrayPool = JsonArrayPool.Instance
        };
        JsonSerializer serializer = JsonSerializer.CreateDefault(_settings);
        return serializer.Deserialize(jr, objectType);
    }
}