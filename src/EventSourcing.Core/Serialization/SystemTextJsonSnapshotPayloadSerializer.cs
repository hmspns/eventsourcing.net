﻿using System;
using System.Text.Json;
using EventSourcing.Abstractions.Contracts;

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
            throw new ArgumentNullException(nameof(obj));
        }

        type = obj.GetType().AssemblyQualifiedName;

        byte[] data = JsonSerializer.SerializeToUtf8Bytes(obj, _options);
        return data;
    }

    public object Deserialize(byte[] data, string type)
    {
        Type t = Type.GetType(type);

        Span<byte> span = data;

        object result = JsonSerializer.Deserialize(span, t, _options);
        return result;
    }
}