using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Abstractions.Types;

namespace EventSourcing.Storage.Redis;

public sealed class RedisSnapshotStoreResolver : IResolveSnapshotStore
{
    private readonly IRedisConnection _redisConnection;
    private readonly ISnapshotsSerializerFactory _serializerFactory;
    private readonly RedisSnapshotCreationPolicy _redisSnapshotCreationPolicy;
    private readonly IRedisKeyGenerator _keyGenerator;

    public RedisSnapshotStoreResolver(
        IRedisConnection redisConnection,
        ISnapshotsSerializerFactory serializerFactory,
        IRedisKeyGenerator keyGenerator,
        RedisSnapshotCreationPolicy redisSnapshotCreationPolicy)
    {
        _keyGenerator = keyGenerator;
        _redisSnapshotCreationPolicy = redisSnapshotCreationPolicy;
        _serializerFactory = serializerFactory;
        _redisConnection = redisConnection;
    }

    public ISnapshotStore Get(TenantId tenantId)
    {
        return new RedisSnapshotStore(_redisConnection, _serializerFactory, _redisSnapshotCreationPolicy, _keyGenerator, tenantId);
    }
}