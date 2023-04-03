using System.Runtime.CompilerServices;
using EventSourcing.Abstractions.Identities;
using StackExchange.Redis;

namespace EventSourcing.Storage.Redis;

/// <summary>
/// Generator to generate redis key.
/// </summary>
public interface IRedisKeyGenerator
{
    /// <summary>
    /// Get redis key by <paramref name="tenantId"/> and <paramref name="streamName"/>.
    /// </summary>
    /// <param name="tenantId">Id of tenant.</param>
    /// <param name="streamName">Name of the events stream.</param>
    /// <returns>Redis key to set/retrieve a snapshot.</returns>
    RedisKey GetKey(TenantId tenantId, StreamId streamName);
}

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
            key = prefix + "|" + tenantId.Id + "|" + streamName;
        }
        else
        {
            key = prefix + "|" + streamName;
        }
        return key;
    }
}