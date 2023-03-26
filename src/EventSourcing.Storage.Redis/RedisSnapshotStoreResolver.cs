using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using StackExchange.Redis;

namespace EventSourcing.Storage.Redis;

public sealed class RedisSnapshotStoreResolver : IResolveSnapshotStore
{
    private readonly IRedisConnection _redisConnection;
    private IPayloadSerializer _payloadSerializer;

    public RedisSnapshotStoreResolver(IRedisConnection redisConnection, IPayloadSerializer payloadSerializer)
    {
        _payloadSerializer = payloadSerializer;
        _redisConnection = redisConnection;
    }
    
    public ISnapshotStore Get(TenantId tenantId)
    {
        return new RedisSnapshotStore(_redisConnection, _payloadSerializer, tenantId);
    }
}

public sealed class RedisSnapshotStore : ISnapshotStore
{
    private readonly IRedisConnection _redisConnection;
    private readonly TenantId _tenantId;
    private IPayloadSerializer _payloadSerializer;

    internal RedisSnapshotStore(IRedisConnection redisConnection, IPayloadSerializer payloadSerializer, TenantId tenantId)
    {
        _payloadSerializer = payloadSerializer;
        _tenantId = tenantId;
        _redisConnection = redisConnection;
    }
    
    public Task<ISnapshot> LoadSnapshot(StreamId streamName)
    {
        throw new NotImplementedException();
    }

    public Task SaveSnapshot(StreamId streamName, ISnapshot snapshot)
    {
        // try
        // {
        //     IDatabaseAsync database = _redisConnection.Connection.GetDatabase();
        //     RedisValue value = _payloadSerializer.Serialize(snapshot, out strin);
        //     bool isSuccess = await database.StringSetAsync(key, value, expire);
        //     return isSuccess;
        // }
        // catch (Exception e)
        // {
        //     HandleException(key, e, data);
        //     return false;
        // }
        return Task.CompletedTask;
    }
}