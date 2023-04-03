using System;
using System.Text.Json;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Core.Exceptions;

namespace EventSourcing.Core.Serialization;

/// <inheritdoc />
public sealed class SystemTextJsonSnapshotPayloadSerializer : IPayloadSerializer
{
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions()
    {
        WriteIndented = false
    };
    
    public byte[] Serialize(object obj, out string type)
    {
        if (obj == null)
        {
            Thrown.ArgumentNullException(nameof(obj));
        }

        type = obj.GetType().AssemblyQualifiedName;

        byte[] data = JsonSerializer.SerializeToUtf8Bytes(obj, _options);
        return data;
    }

    public object Deserialize(Memory<byte> data, string type)
    {
        Type t = Type.GetType(type);

        Span<byte> span = data.Span;

        object result = JsonSerializer.Deserialize(span, t, _options);
        return result;
    }
}