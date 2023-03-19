﻿using System.Buffers;
using System.Text;
using EventSourcing.Abstractions;
using Newtonsoft.Json;

namespace EventSourcing.Serialization.Newtonsoft;

/// <inheritdoc />
public class JsonPayloadSerializer : IPayloadSerializer
{
    private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings()
    {
        Formatting = Formatting.Indented,
        Converters = new List<JsonConverter>()
        {
            new IdentityConverter()
        }
    };

    private static readonly Dictionary<string, Type> _mappings = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(x => x.GetTypes())
        .Where(x => x.IsAssignableTo(typeof(IEvent))).ToDictionary(x => x.FullName, x => x);
    
    public byte[] Serialize(object obj, out string type)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj));
        }
        
        type = obj.GetType().FullName;
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
        
        if (!_mappings.TryGetValue(type, out Type t))
        {
            throw new ArgumentOutOfRangeException(nameof(type), "Couldn't proper type for Event");
        }

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