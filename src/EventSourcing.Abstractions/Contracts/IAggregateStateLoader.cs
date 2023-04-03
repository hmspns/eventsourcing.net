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
    /// Return state for specific aggregate.
    /// </summary>
    /// <param name="tenantId">Tenant id.</param>
    /// <param name="aggregateId">Aggregate id.</param>
    /// <param name="useSnapshot">Should snapshot be used to create state.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <returns>Current state of the aggregate.</returns>
    Task<TState> GetState(TenantId tenantId, TId aggregateId, bool useSnapshot = true);
}