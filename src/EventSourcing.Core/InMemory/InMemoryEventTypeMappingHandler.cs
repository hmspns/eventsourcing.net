using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EventSourcing.Abstractions.Contracts;

namespace EventSourcing.Core.InMemory;

/// <inheritdoc />
public class InMemoryEventTypeMappingHandler : IEventTypeMappingHandler
{
    private static readonly IReadOnlyDictionary<string, Type> _mappings = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(x => x.GetTypes())
        .Where(x => x.IsAssignableTo(typeof(IEvent)))
        .ToDictionary(x => x.FullName, x => x, StringComparer.Ordinal);
    
    public Type GetEventType(string stringRepresentation)
    {
        if (stringRepresentation == null)
        {
            throw new ArgumentNullException(nameof(stringRepresentation));
        }
        
        if (!_mappings.TryGetValue(stringRepresentation, out Type type))
        {
            throw new ArgumentOutOfRangeException(nameof(stringRepresentation),
                $"Couldn't find proper type of event for '{stringRepresentation}'");
        }

        return type;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual string GetStringRepresentation(Type type)
    {
        return type.FullName;
    }
}