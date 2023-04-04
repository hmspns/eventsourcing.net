using System;
using System.Threading.Tasks;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Abstractions.Types;

namespace EventSourcing.Core.NoImplementation;

/// <inheritdoc />
public sealed class NoSnapshotStoreResolver : IResolveSnapshotStore
{
    private readonly NoSnapshotStore _store = new NoSnapshotStore();
    
    public ISnapshotStore Get(TenantId tenantId)
    {
        return _store;
    }
}
    
/// <summary>
/// Store that produce no snapshots.
/// </summary>
public sealed class NoSnapshotStore : ISnapshotStore
{
    /// <summary>
    /// Load snapshot.
    /// </summary>
    /// <param name="streamName">Stream name.</param>
    /// <returns>Snapshot data.</returns>
    public Task<ISnapshot> LoadSnapshot(StreamId streamName)
    {
        return Task.FromResult<ISnapshot>(new Snapshot(streamName, null, AggregateVersion.NotCreated));
    }

    /// <summary>
    /// Save snapshot to store.
    /// </summary>
    /// <param name="streamName">Stream name.</param>
    /// <param name="snapshot">Snapshot data.</param>
    public Task SaveSnapshot(StreamId streamName, ISnapshot snapshot)
    {
        return Task.CompletedTask;
    }
}