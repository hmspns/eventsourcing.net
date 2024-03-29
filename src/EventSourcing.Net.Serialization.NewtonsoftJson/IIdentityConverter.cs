﻿using EventSourcing.Net.Abstractions.Contracts;
using Newtonsoft.Json;

namespace EventSourcing.Net.Serialization.NewtonsoftJson;

public sealed class IIdentityConverter : JsonConverter<IIdentity>
{
    public override void WriteJson(JsonWriter writer, IIdentity? value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.ToString());
    }

    public override IIdentity? ReadJson(JsonReader reader, Type objectType, IIdentity? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.Value != null)
        {
            string? raw = reader.Value?.ToString();
            return IIdentity.Parse(raw);
        }

        return null;
    }
}