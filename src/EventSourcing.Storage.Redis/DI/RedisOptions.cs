using EventSourcing.Net;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Storage.Redis;

public sealed class RedisOptions
{
    private readonly IServiceCollection _services;

    internal RedisOptions(IServiceCollection services)
    {
        _services = services;
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
        _services.Remove<IRedisKeyGenerator>();
        _services.AddSingleton<IRedisKeyGenerator>(keyGenerator);
        return this;
    }
}