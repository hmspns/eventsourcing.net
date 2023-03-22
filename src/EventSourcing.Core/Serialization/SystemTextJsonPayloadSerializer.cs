using System;
using System.Text.Json;
using System.Threading;
using EventSourcing.Abstractions;
using EventSourcing.Abstractions.Contracts;

namespace EventSourcing.Core.Serialization;

/// <inheritdoc />
public sealed class SystemTextJsonPayloadSerializer : IPayloadSerializer
{
    private readonly IEventTypeMappingHandler _eventTypeMappingHandler;
    
    private static JsonSerializerOptions _options = new JsonSerializerOptions()
    {
        WriteIndented = true,
        Converters = { new IIdentityConverter() }
    };

    /// <summary>
    /// Get or set serializing options.
    /// </summary>
    public static JsonSerializerOptions SerializationOptions
    {
        get => _options;
        set => Interlocked.Exchange(ref _options, value);
    }

    public SystemTextJsonPayloadSerializer(IEventTypeMappingHandler eventTypeMappingHandler)
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

        byte[] data = JsonSerializer.SerializeToUtf8Bytes(obj, _options);
        return data;
    }

    public object Deserialize(byte[] data, string type)
    {
        Type t = _eventTypeMappingHandler.GetEventType(type);

        Span<byte> span = data;

        object result = JsonSerializer.Deserialize(span, t, _options);
        return result;
    }
}