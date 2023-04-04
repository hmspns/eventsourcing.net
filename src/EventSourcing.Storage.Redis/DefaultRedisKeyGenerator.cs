using System.Runtime.CompilerServices;
using EventSourcing.Abstractions.Identities;
using StackExchange.Redis;

namespace EventSourcing.Storage.Redis;

/// <inheritdoc />
internal sealed class DefaultRedisKeyGenerator : IRedisKeyGenerator
{
    private readonly RedisSnapshotCreationPolicy _policy;

    public DefaultRedisKeyGenerator(RedisSnapshotCreationPolicy policy)
    {
        _policy = policy;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RedisKey GetKey(TenantId tenantId, StreamId streamName)
    {
        string prefix = _policy?.KeyPrefix ?? "es";
        RedisKey key;
        if (tenantId != TenantId.Empty)
        {
            key = prefix + "|" + tenantId.Id.ToString() + "|" + streamName;
        }
        else
        {
            key = prefix + "|" + streamName;
        }
        return key;
    }
}