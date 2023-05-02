using System.Threading.Tasks;
using EventSourcing.Net.Abstractions.Identities;

namespace EventSourcing.Net.Abstractions.Contracts;

/// <summary>
/// Service to initialize tenant storage.
/// </summary>
public interface IEventSourcingStorage
{
    /// <summary>
    /// Initialize storage for tenant.
    /// </summary>
    /// <param name="tenantId">Id of tenant.</param>
    Task Initialize(TenantId tenantId);

    /// <summary>
    /// Initialize storage for non multitenancy.
    /// </summary>
    Task Initialize() => Initialize(TenantId.Empty);
}