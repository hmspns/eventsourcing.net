using System;

namespace EventSourcing.Abstractions.Contracts;

/// <summary>
/// Provide functionality to resolve type of event by string representation.
/// </summary>
[Obsolete]
public interface IEventTypeMappingHandler
{
    /// <summary>
    /// Get correct type by string representation of type.
    /// </summary>
    /// <param name="stringRepresentation">String representation of type.</param>
    /// <returns>Event type.</returns>
    Type GetEventType(string stringRepresentation);

    /// <summary>
    /// Get string representation of specific type.
    /// </summary>
    /// <param name="type">Type of event.</param>
    /// <returns>String representation of type.</returns>
    string GetStringRepresentation(Type type);
}