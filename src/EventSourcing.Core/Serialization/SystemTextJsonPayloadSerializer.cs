using System;
using System.Text.Json;
using System.Threading;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Core.Exceptions;
using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Core.Serialization;

/// <inheritdoc />
public sealed class SystemTextJsonPayloadSerializer : IPayloadSerializer
{
    private readonly JsonSerializerOptions _options;

    public SystemTextJsonPayloadSerializer(JsonSerializerOptions options)
    {
        _options = options;
    }
    
    public byte[] Serialize(object obj)
    {
        if (obj == null)
        {
            Thrown.ArgumentNullException(nameof(obj));
        }
        
        byte[] data = JsonSerializer.SerializeToUtf8Bytes(obj, _options);
        return data;
    }

    public object Deserialize(Type type, Memory<byte> data)
    {
        Span<byte> span = data.Span;

        object? result = JsonSerializer.Deserialize(span, type, _options);
        if (result == null)
        {
            Thrown.InvalidOperationException("Couldn't deserialize ");
        }
        return result;
    }
}