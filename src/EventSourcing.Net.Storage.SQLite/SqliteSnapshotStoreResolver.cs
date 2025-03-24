namespace EventSourcing.Net.Storage.SQLite;

using Abstractions.Contracts;
using Abstractions.Identities;

public sealed class SqliteSnapshotStoreResolver : IResolveSnapshotStore
{
    private readonly string _connectionString;
    private readonly IPayloadSerializer _payloadSerializer;

    public SqliteSnapshotStoreResolver(string connectionString, IPayloadSerializer payloadSerializer)
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