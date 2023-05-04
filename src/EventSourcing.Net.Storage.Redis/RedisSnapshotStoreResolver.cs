using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;

namespace EventSourcing.Net.Storage.Redis;

public sealed class RedisSnapshotStoreResolver : IResolveSnapshotStore
{
    private readonly IRedisConnection _redisConnection;
    private readonly ISnapshotsSerializerFactory _serializerFactory;
    private readonly RedisSnapshotCreationPolicy _redisSnapshotCreationPolicy;
    private readonly IRedisKeyGenerator _keyGenerator;
    private readonly ITypeMappingHandler _typeMappingHandler;

    public RedisSnapshotStoreResolver(
        IRedisConnection redisConnection,
        ISnapshotsSerializerFactory serializerFactory,
        IRedisKeyGenerator keyGenerator,
        ITypeMappingHandler typeMappingHandler,
        RedisSnapshotCreationPolicy redisSnapshotCreationPolicy)
    {
        _typeMappingHandler = typeMappingHandler;
        _keyGenerator = keyGenerator;
        _redisSnapshotCreationPolicy = redisSnapshotCreationPolicy;
        _serializerFactory = serializerFactory;
        _redisConnection = redisConnection;
    }

    public ISnapshotStore Get(TenantId tenantId)
    {
        return new RedisSnapshotStore(_redisConnection, _serializerFactory, _redisSnapshotCreationPolicy, _keyGenerator, _typeMappingHandler, tenantId);
    }
}