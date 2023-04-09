using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Abstractions.Contracts;

/// <summary>
/// Provide functionality to track relation between types and it's id.
/// </summary>
public interface ITypeMappingHandler : IDisposable
{
    /// <summary>
    /// Get type by id.
    /// </summary>
    /// <param name="id">Id of type.</param>
    /// <returns>Type matched to id.</returns>
    Type GetTypeById(TypeMappingId id);

    /// <summary>
    /// Get id for type.
    /// </summary>
    /// <param name="type">Type to get id.></param>
    /// <returns></returns>
    TypeMappingId GetIdByType(Type type);

    internal void SetStorageTypes(IEnumerable<Type> types);

    /// <summary>
    /// Load 
    /// </summary>
    /// <param name="types"></param>
    internal Task SynchronizeAppTypesWithStorageTypes();
}