using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using EventSourcing.Net.Engine.Exceptions;

namespace EventSourcing.Net.Engine;

using Abstractions.Contracts;

internal sealed class AggregateIdParsingProvider : IAggregateIdParsingProvider
{
    private readonly ConcurrentDictionary<Type, Func<string, object>> _handlers = new ConcurrentDictionary<Type, Func<string, object>>();

    public TId Parse<TId>(string value)
    {
        Type? idType = typeof(TId);
        if (!_handlers.TryGetValue(idType, out Func<string, object?>? handler))
        {
            handler = RegisterHandler(idType);
        }

        return (TId)handler(value);
    }

    public Func<string, object> GetParser(Type idType)
    {
        if (!_handlers.TryGetValue(idType, out Func<string, object?>? parser))
        {
            parser = RegisterHandler(idType);
        }

        return parser!;
    }

    private Func<string, object?> RegisterHandler(Type idType)
    {
        TypeConverterAttribute? attribute = idType.GetCustomAttribute<TypeConverterAttribute>();
        TypeConverter? converter = null;
        if (attribute != null)
        {
            Type? converterType = Type.GetType(attribute.ConverterTypeName);
            if (converterType != null)
            {
                converter = (TypeConverter?)Activator.CreateInstance(converterType);
            }
        }
        else
        {
            converter = TypeDescriptor.GetConverter(idType);
        }

        if (converter == null)
        {
            Thrown.InvalidOperationException("Couldn't find type converter for " + idType.AssemblyQualifiedName);
        }

        if (!converter.CanConvertFrom(typeof(string)))
        {
            Thrown.InvalidOperationException("Converter doesn't support converting from string to " + idType.AssemblyQualifiedName);
        }
        
        Func<string, object> handler = (x) =>
        {
            object? value = converter.ConvertFromString(x);
            if (value == null)
            {
                Thrown.InvalidOperationException("AggregateId couldn't be null");
            }

            return value;
        };
        _handlers.TryAdd(idType, handler);
        return handler;
    }
}