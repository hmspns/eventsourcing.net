﻿using System.Text.Json;
using System.Text.Json.Serialization;
using EventSourcing.Abstractions.Contracts;

namespace EventSourcing.Serialization.SystemText;

internal sealed class IIdentityConverter : JsonConverter<IIdentity>
{
    public override IIdentity? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? raw = reader.GetString();
        if (raw == null)
        {
            return null;
        }
        return IIdentity.Parse(raw);
    }

    public override void Write(Utf8JsonWriter writer, IIdentity value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}