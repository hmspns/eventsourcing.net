﻿using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Types;
using EventSourcing.Core.Exceptions;
using EventSourcing.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EventSourcing.Storage.Redis;

public static class EventSourcingOptionsExtensions
{
    public static RedisOptions UseRedisSnapshotStore(
        this EventSourcingOptions options,
        string connectionString,
        RedisSnapshotCreationPolicy? policy = null)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (connectionString == null)
        {
            Thrown.ArgumentNullException(nameof(connectionString));
        }

        if (policy == null)
        {
            policy = new RedisSnapshotCreationPolicy();
        }

        options._services.Replace<IRedisKeyGenerator>(x =>
        {
            x.AddSingleton<IRedisKeyGenerator>(x => new DefaultRedisKeyGenerator(policy));
        });
        options._services.Replace<IResolveSnapshotStore>(x =>
        {
            x.AddSingleton<IResolveSnapshotStore>(x =>
            {
                RedisConnection connection = new RedisConnection(connectionString);
                ISnapshotsSerializerFactory serializerFactory = x.GetRequiredService<ISnapshotsSerializerFactory>();
                IRedisKeyGenerator keyGenerator = x.GetRequiredService<IRedisKeyGenerator>();
                return new RedisSnapshotStoreResolver(
                    connection,
                    serializerFactory,
                    keyGenerator,
                    policy
                );
            });
        });

        return new RedisOptions(options._services);
    }
}

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