using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using EventSourcing.Net.Engine.Exceptions;

namespace EventSourcing.Net.Engine;

internal sealed class AggregateIdParsingProvider
{
    #region Singleton

    private static readonly AggregateIdParsingProvider _instance = new AggregateIdParsingProvider();

    internal static AggregateIdParsingProvider Instance => _instance;

    private AggregateIdParsingProvider()
    {
    }

    #endregion

    private readonly ConcurrentDictionary<Type, Func<string, object?>> _handlers = new ConcurrentDictionary<Type, Func<string, object?>>();

    internal TId Parse<TId>(string value)
    {
        Type? idType = typeof(TId);
        if (!_handlers.TryGetValue(idType, out Func<string, object?>? handler))
        {
            handler = RegisterHandler<TId>();
        }

        return (TId)handler(value);
    }

    internal Func<string, object> GetParser<TId>()
    {
        Type idType = typeof(TId);
        if (!_handlers.TryGetValue(idType, out Func<string, object?>? parser))
        {
            parser = RegisterHandler<TId>();
        }

        return parser!;
    }
    
    private Func<string, object?> RegisterHandler<TId>()
    {
        Type idType = typeof(TId);
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
        
        Func<string, object?> handler = (x) => converter.ConvertFromString(x);
        _handlers.TryAdd(idType, handler);
        return handler;
    }
}