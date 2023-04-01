using EventSourcing.Abstractions.Contracts;
using EventSourcing.Net;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Storage.Redis;

public static class EventSourcingOptionsExtensions
{
    public static EventSourcingOptions UseRedisSnapshotStore(this EventSourcingOptions options, string connectionString)
    {
        if (connectionString == null)
        {
            throw new ArgumentNullException(nameof(connectionString));
        }
        options._services.Remove<IResolveSnapshotStore>();
        options._services.AddSingleton<IResolveSnapshotStore>(x =>
        {
            RedisConnection connection = new RedisConnection(connectionString);
            ISnapshotsSerializerFactory serializerFactory = x.GetRequiredService<ISnapshotsSerializerFactory>();
            return new RedisSnapshotStoreResolver(connection, serializerFactory);
        });

        return options;
    }
}