using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using EventSourcing.Net.Engine.Exceptions;

namespace EventSourcing.Net.Engine;

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
        
        #if NET7_0_OR_GREATER

        // Func<string, object?> handler7 = TryCreateIParsableBinder<TId>(idType);
        // if (handler7 != null)
        // {
        //     _handlers.TryAdd(idType, handler7);
        //     return handler7;
        // }
        
        #endif
        
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

    #if NET7_0_OR_GREATER
    
    private Func<string, object> TryCreateIParsableBinder<TId>(Type idType)
    {
        Type[] interfaces = idType.GetInterfaces();
        Type? inter = interfaces.FirstOrDefault(x => x.FullName.Contains("System.IParsable", StringComparison.Ordinal));
        if (inter != null)
        {
            MethodInfo? method = inter.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static);
            if (method != null)
            {
                var tmp = method.Invoke(null, new object?[] { "tst", null });
                Func<string, object> h = x => method.Invoke(null, new object?[] { x, null });
                return h;
            }
        }

        return null;
    }
    #endif
    
    public Delegate CreateDelegate(MethodInfo methodInfo)
    {
        IEnumerable<Type> parmTypes = methodInfo.GetParameters().Select(parm => parm.ParameterType);
        Type[] parmAndReturnTypes = parmTypes.Append(methodInfo.ReturnType).ToArray();
        Type delegateType = Expression.GetDelegateType(parmAndReturnTypes);

        return methodInfo.CreateDelegate(delegateType);
    }


}