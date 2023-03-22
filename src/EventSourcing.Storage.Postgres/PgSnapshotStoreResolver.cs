using EventSourcing.Abstractions;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Core.Contracts;

namespace EventSourcing.Storage.Postgres;

public sealed class PgSnapshotStoreResolver : IResolveSnapshotStore
{
    private readonly string _connectionString;
    private readonly IPayloadSerializer _payloadSerializer;

    public PgSnapshotStoreResolver(string connectionString, IPayloadSerializer payloadSerializer)
    {
        _payloadSerializer = payloadSerializer;
        _connectionString = connectionString;
    }
    
    public ISnapshotStore Get(TenantId tenantId)
    {
        throw new NotImplementedException();
    }
}

public sealed class PgSnapshotStore : ISnapshotStore
{
    public Task<ISnapshot> LoadSnapshot(StreamId streamName)
    {
        throw new NotImplementedException();
    }

    public Task SaveSnapshot(StreamId streamName, ISnapshot snapshot)
    {
        throw new NotImplementedException();
    }
}