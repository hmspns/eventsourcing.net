using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Core.Exceptions;

namespace EventSourcing.Net.Core.InMemory;

/// <inheritdoc />
public sealed class DefaultTypeStringConverter : ITypeStringConverter
{
    private readonly ConcurrentDictionary<string?, Type> _typeCache = new(StringComparer.Ordinal);

    public DefaultTypeStringConverter()
    {
        
    }
    
    public DefaultTypeStringConverter(IEnumerable<Type> assemblyTypes)
    {
        if (assemblyTypes != null)
        {
            foreach (Type type in assemblyTypes)
            {
                string representation = GetStringRepresentation(type);
                _typeCache.TryAdd(representation, type);
            }
        }
    }

    public Type GetType(string? stringRepresentation)
    {
        if (stringRepresentation == null)
        {
            Thrown.ArgumentNullException(nameof(stringRepresentation));
        }

        if (_typeCache.TryGetValue(stringRepresentation, out Type? type))
        {
            return type;
        }

        type = Type.GetType(stringRepresentation, AssemblyResolver, null);
        if (type == null)
        {
            Thrown.InvalidOperationException($"Couldn't load type for '{stringRepresentation}'");
        }

        _typeCache.TryAdd(stringRepresentation, type);

        return type;
    }

    public string GetStringRepresentation(Type type)
    {
        string? representation = type.AssemblyQualifiedName;
        if (representation == null)
        {
            Thrown.InvalidOperationException($"Property FullName of type '{type.ToString()}' return null");
        }
        return representation;
    }
    
    private static Assembly AssemblyResolver(AssemblyName assemblyName)
    {
        assemblyName.Version = null;
        return Assembly.Load(assemblyName);
    }
}