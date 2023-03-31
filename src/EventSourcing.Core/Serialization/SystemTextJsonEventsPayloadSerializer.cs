using System;
using System.Text.Json;
using System.Threading;
using EventSourcing.Abstractions.Contracts;

namespace EventSourcing.Core.Serialization;

/// <inheritdoc />
public sealed class SystemTextJsonEventsPayloadSerializer : IPayloadSerializer
{
    private readonly IEventTypeMappingHandler _eventTypeMappingHandler;
    
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions()
    {
        WriteIndented = true,
        Converters = { new IIdentityConverter() }
    };
    
    public SystemTextJsonEventsPayloadSerializer(IEventTypeMappingHandler eventTypeMappingHandler)
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

    public object Deserialize(Memory<byte> data, string type)
    {
        Type t = _eventTypeMappingHandler.GetEventType(type);

        Span<byte> span = data.Span;

        object result = JsonSerializer.Deserialize(span, t, _options);
        return result;
    }
}