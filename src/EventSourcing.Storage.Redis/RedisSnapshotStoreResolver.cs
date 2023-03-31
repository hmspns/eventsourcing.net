using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Storage.Redis;

public sealed class RedisSnapshotStoreResolver : IResolveSnapshotStore
{
    private readonly IRedisConnection _redisConnection;
    private readonly ISnapshotsSerializerFactory _serializerFactory;

    public RedisSnapshotStoreResolver(IRedisConnection redisConnection, ISnapshotsSerializerFactory serializerFactory)
    {
        _serializerFactory = serializerFactory;
        _redisConnection = redisConnection;
    }

    public ISnapshotStore Get(TenantId tenantId)
    {
        return new RedisSnapshotStore(_redisConnection, _serializerFactory, tenantId);
    }
}