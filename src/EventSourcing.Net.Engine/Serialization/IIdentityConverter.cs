using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Net.Engine.Serialization;

#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Abstractions.Identities;

public sealed class IdentityJsonConverterFactory : JsonConverterFactory
{
    private static readonly IIdentityConverter _converter = new IIdentityConverter();

    private static readonly ISet<Type> _supportedTypes = new List<Type>()
    {
        typeof(CommandId),
        typeof(CommandSequenceId),
        typeof(EventId),
        typeof(PrincipalId),
        typeof(TenantId),
        typeof(TypeMappingId),
#if NET8_0_OR_GREATER
    }.ToFrozenSet();
#else
    }.ToHashSet();
#endif

    public override bool CanConvert(Type typeToConvert)
    {
        return _supportedTypes.Contains(typeToConvert);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return _converter;
    }
}

internal sealed class IIdentityConverter : JsonConverter<IIdentity>
{
    public override IIdentity? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
#if NET7_0_OR_GREATER
        if (!reader.ValueIsEscaped && !reader.HasValueSequence)
        {
            ReadOnlySpan<byte> span = reader.ValueSpan;

            int charsCount = Encoding.UTF8.GetMaxCharCount(span.Length);
            if (charsCount < 128)
            {
                Span<char> chars = stackalloc char[charsCount];
                int realCount = Encoding.UTF8.GetChars(span, chars);
                chars = chars.Slice(0, realCount);
                return IIdentity.Parse(chars);
            }
        }
#endif

        string? raw = reader.GetString();
        if (raw == null)
        {
            return null;
        }
        return IIdentity.Parse(raw);
    }

    public override void Write(Utf8JsonWriter writer, IIdentity value, JsonSerializerOptions options)
    {
        if (value is ISpanFormattable spanFormattable)
        {
            Span<char> buffer = stackalloc char[100]; // if buffer will be too small, TryFormat return 'false'
            if (spanFormattable.TryFormat(buffer, out int charsWritten, ReadOnlySpan<char>.Empty, null))
            {
                buffer = buffer.Slice(0, charsWritten);
                writer.WriteStringValue(buffer);
                return;
            }
        }
        writer.WriteStringValue(value.ToString());
    }
}