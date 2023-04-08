using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Core.Exceptions;

namespace EventSourcing.Core.InMemory;

/// <inheritdoc />
public sealed class InMemoryEventTypeMappingHandler : IEventTypeMappingHandler
{
    private readonly IReadOnlyDictionary<string, Type> _mappings;

    public InMemoryEventTypeMappingHandler(IReadOnlyDictionary<string, Type> mappings)
    {
        _mappings = mappings;
    }

    public Type GetEventType(string stringRepresentation)
    {
        if (stringRepresentation == null)
        {
            Thrown.ArgumentNullException(nameof(stringRepresentation));
        }
        
        if (!_mappings.TryGetValue(stringRepresentation, out Type type))
        {
            throw new ArgumentOutOfRangeException(nameof(stringRepresentation),
                $"Couldn't find proper type of event for '{stringRepresentation}'");
        }

        return type;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetStringRepresentation(Type type)
    {
        return type.FullName;
    }
}