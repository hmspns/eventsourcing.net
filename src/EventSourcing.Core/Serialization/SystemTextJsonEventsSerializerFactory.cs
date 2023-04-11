﻿using System.Text.Json;
using System.Text.Json.Serialization;
using EventSourcing.Abstractions.Contracts;

namespace EventSourcing.Core.Serialization;

/// <inheritdoc />
public sealed class SystemTextJsonEventsSerializerFactory : IEventsPayloadSerializerFactory
{
    private readonly SystemTextJsonPayloadSerializer _serializer;
    
    public SystemTextJsonEventsSerializerFactory(JsonSerializerOptions? options = null)
    {
        options ??= new JsonSerializerOptions()
        {
            WriteIndented = false,
            Converters =
            {
                new IIdentityConverter(),
                new JsonStringEnumConverter()
            }
        };
        _serializer = new SystemTextJsonPayloadSerializer(options);
    }

    public IPayloadSerializer GetSerializer()
    {
        return _serializer;
    }
}