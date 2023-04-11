using System.Threading.Tasks;
using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Abstractions.Contracts;

/// <summary>
/// Factory to resolve tenant specific snapshot store.
/// </summary>
public interface IResolveSnapshotStore
{
    /// <summary>
    /// Return tenant specific snapshot store.
    /// </summary>
    /// <param name="tenantId">Tenant id.</param>
    /// <returns>Snapshot store.</returns>
    ISnapshotStore Get(TenantId tenantId);
}
    
/// <summary>
/// Snapshot store.
/// </summary>
public interface ISnapshotStore
{
    /// <summary>
    /// Load snapshot.
    /// </summary>
    /// <param name="streamName">Stream name.</param>
    /// <returns>Snapshot data.</returns>
    Task<ISnapshot> LoadSnapshot(StreamId streamName);

    /// <summary>
    /// Save snapshot to store.
    /// </summary>
    /// <param name="streamName">Stream name.</param>
    /// <param name="snapshot">Snapshot data.</param>
    Task SaveSnapshot(StreamId streamName, ISnapshot snapshot);
}