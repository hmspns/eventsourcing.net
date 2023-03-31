using System.Threading.Tasks;
using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Abstractions.Contracts;

/// <summary>
/// Load state for an aggregate.
/// </summary>
/// <typeparam name="TId">Aggregate id type.</typeparam>
/// <typeparam name="TState">State type.</typeparam>
public interface IAggregateStateLoader<in TId, TState>
{
    /// <summary>
    /// Return state for a specific aggregate.
    /// </summary>
    /// <param name="tenantId">Tenant id.</param>
    /// <param name="aggregateId">Aggregate id.</param>
    /// <returns>Aggregate state.</returns>
    Task<TState> GetState(TenantId tenantId, TId aggregateId);
}