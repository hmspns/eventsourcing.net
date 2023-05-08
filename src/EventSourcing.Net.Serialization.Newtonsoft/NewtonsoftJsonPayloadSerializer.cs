using System.Buffers;
using EventSourcing.Net.Abstractions.Contracts;
using Newtonsoft.Json;

namespace EventSourcing.Net.Serialization.Newtonsoft;

/// <inheritdoc />
public sealed class NewtonsoftJsonPayloadSerializer : IPayloadSerializer
{
    private readonly JsonSerializerSettings _settings;
    
    public NewtonsoftJsonPayloadSerializer(JsonSerializerSettings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// Serialize data.
    /// </summary>
    /// <param name="obj">Data to be serialized.</param>
    /// <returns>Serialized data.</returns>
    public byte[] Serialize(object obj)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj));
        }
        return SerializeJson(obj);
    }

    /// <summary>
    /// Deserialize data.
    /// </summary>
    /// <param name="type">Type of serialized data.</param>
    /// <param name="data">Binary data.</param>
    /// <returns>Deserialized object.</returns>
    public object Deserialize(Type type, Memory<byte> data)
    {
        if (data.IsEmpty)
        {
            throw new ArgumentException("Data is empty", nameof(data));
        }

        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }
        
        object? result = DeserializeJson(type, data);
        if (result == null)
        {
            throw new InvalidOperationException($"Couldn't deserialize type {type}");
        }

        return result;
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
    
    private object? DeserializeJson(Type objectType, Memory<byte> content)
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