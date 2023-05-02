using EventSourcing.Net.Abstractions.Contracts;
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

        options.Services.Replace<IRedisKeyGenerator>(x =>
        {
            x.AddSingleton<IRedisKeyGenerator>(x => new DefaultRedisKeyGenerator(policy));
        });
        options.Services.Replace<IResolveSnapshotStore>(x =>
        {
            x.AddSingleton<IResolveSnapshotStore>(x =>
            {
                RedisConnection connection = new RedisConnection(connectionString);
                ISnapshotsSerializerFactory serializerFactory = x.GetRequiredService<ISnapshotsSerializerFactory>();
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
        });

        return new RedisOptions(options);
    }
}