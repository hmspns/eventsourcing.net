using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Net.Abstractions.Types;

namespace EventSourcing.Net.Abstractions.Contracts;

/// <summary>
/// Provider to load/store types from storage.
/// </summary>
public interface ITypeMappingStorageProvider
{
    /// <summary>
    /// Initialize storage.
    /// </summary>
    Task Initialize();

    /// <summary>
    /// Load existing mappings from storage.
    /// </summary>
    /// <returns>Mappings that present in the storage.</returns>
    Task<IReadOnlyCollection<TypeMapping>> GetMappings();

    /// <summary>
    /// Add mappings to storage.
    /// </summary>
    /// <param name="mappings">Mappings that should be added.</param>
    Task AddMappings(IEnumerable<TypeMapping> mappings);
}