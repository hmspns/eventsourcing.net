using System;

namespace EventSourcing.Net.Abstractions.Contracts;

/// <summary>
/// Provide functionality to resolve type of event by string representation.
/// </summary>
public interface ITypeStringConverter
{
    /// <summary>
    /// Get correct type by string representation of type.
    /// </summary>
    /// <param name="stringRepresentation">String representation of type.</param>
    /// <returns>Event type.</returns>
    Type GetType(string? stringRepresentation);

    /// <summary>
    /// Get string representation of type to store it in the storage.
    /// </summary>
    /// <param name="type">Type to get its string representation.</param>
    /// <returns>String representation of type.</returns>
    /// <exception cref="InvalidOperationException">AssemblyQualifiedName of <paramref name="type"/> returns null.</exception>
    string GetStringRepresentation(Type type);
}