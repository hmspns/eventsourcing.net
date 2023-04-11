using System.Threading.Tasks;
using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Abstractions.Contracts;

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