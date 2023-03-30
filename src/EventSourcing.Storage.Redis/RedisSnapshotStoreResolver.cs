using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Storage.Redis;

public sealed class RedisSnapshotStoreResolver : IResolveSnapshotStore
{
    private readonly IRedisConnection _redisConnection;
    private readonly IPayloadSerializer _payloadSerializer;

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