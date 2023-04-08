using System;
using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Abstractions.Contracts;

/// <summary>
/// Provide functionality to track relation between types and it's id.
/// </summary>
public interface ITypeMappingHandler
{
    /// <summary>
    /// Get type by id.
    /// </summary>
    /// <param name="id">Id of type.</param>
    /// <returns>Type matched to id.</returns>
    Type GetType(TypeMappingId id);

    /// <summary>
    /// Get id for type.
    /// </summary>
    /// <param name="type">Type to get id.></param>
    /// <returns></returns>
    TypeMappingId GetId(Type type);
}