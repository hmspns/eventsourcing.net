using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net.Storage.Redis;

public sealed class RedisOptions
{
    private readonly EventSourcingOptions _options;

    internal RedisOptions(EventSourcingOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Use custom IRedisKeyGenerator.
    /// </summary>
    /// <param name="keyGenerator">Implementation of IRedisKeyGenerator.</param>
    /// <returns>Options.</returns>
    /// <remarks>
    /// Service will be registered as singleton.
    /// Implementation of IRedisKeyGenerator should be thread-safe.
    /// </remarks>
    public RedisOptions UseRedisKeyGenerator(IRedisKeyGenerator keyGenerator)
    {
        _options.Services.Remove<IRedisKeyGenerator>();
        _options.Services.AddSingleton<IRedisKeyGenerator>(keyGenerator);
        return this;
    }
}