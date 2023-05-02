using EventSourcing.Net.Abstractions.Identities;
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