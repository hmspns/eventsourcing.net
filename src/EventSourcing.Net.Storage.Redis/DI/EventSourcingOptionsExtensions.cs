using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Engine.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net.Storage.Redis;

/// <summary>
/// Extensions to configure Redis store.
/// </summary>
public static class EventSourcingOptionsExtensions
{
    /// <summary>
    /// Use Redis as snapshot store.
    /// </summary>
    /// <param name="options">Configuration options.</param>
    /// <param name="connectionString">Redis connections string.</param>
    /// <param name="policy">Redis snapshot creation policy.</param>
    /// <returns>Redis options for additional configuration.</returns>
    public static RedisOptions UseRedisSnapshotStore(
        this EventSourcingStorageOptions options,
        string connectionString,
        RedisSnapshotCreationPolicy? policy = null
        )
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (connectionString == null)
        {
            Thrown.ArgumentNullException(nameof(connectionString));
        }

        policy ??= new RedisSnapshotCreationPolicy();

        options
            .ReplaceSingleton<IRedisKeyGenerator>(new DefaultRedisKeyGenerator(policy))
            .ReplaceSingleton<IResolveSnapshotStore>(x =>
            {
                RedisConnection connection = new RedisConnection(connectionString);
                ISnapshotSerializerFactory serializerFactory = x.GetRequiredService<ISnapshotSerializerFactory>();
                IRedisKeyGenerator keyGenerator = x.GetRequiredService<IRedisKeyGenerator>();
                ITypeMappingHandler typeMappingHandler = x.GetRequiredService<ITypeMappingHandler>();

                return new RedisSnapshotStoreResolver(
                    connection,
                    serializerFactory,
                    keyGenerator,
                    typeMappingHandler,
                    policy
                );
            });

        return new RedisOptions(options);
    }

    /// <summary>
    /// Use Redis as snapshot store.
    /// </summary>
    /// <param name="options">Configuration options.</param>
    /// <param name="connectionString">Redis connections string.</param>
    /// <param name="policy">Redis snapshot creation policy.</param>
    /// <returns>Redis options for additional configuration.</returns>
    [Obsolete("Use EventSourcingOptions.Storage.UseRedisSnapshotStore")]
    public static RedisOptions UseRedisSnapshotStore(
    this EventSourcingOptions options,
    string connectionString,
    RedisSnapshotCreationPolicy? policy = null)
    {
        return options.Storage.UseRedisSnapshotStore(connectionString, policy);
    }
}