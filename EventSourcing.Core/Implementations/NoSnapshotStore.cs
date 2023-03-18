using System;
using System.Threading.Tasks;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Core.Contracts;

namespace EventSourcing.Core.Implementations
{
    /// <inheritdoc />
    public sealed class SnapshotStoreResolver : IResolveSnapshotStore
    {
        public ISnapshotStore Get(TenantId tenantId)
        {
            return new NoSnapshotStore();
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
            return Task.FromResult<ISnapshot>(new Snapshot(streamName, null, AggregateVersion.NotCreated)
            {
                HasSnapshot = false,
                Id = Guid.Empty,
            });
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
}