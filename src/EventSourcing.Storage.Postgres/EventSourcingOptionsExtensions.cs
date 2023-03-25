using EventSourcing.Abstractions;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Net;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Storage.Postgres;

public static class EventSourcingOptionsExtensions
{
    /// <summary>
    /// Use Postgres as events store.
    /// </summary>
    /// <param name="options">Configuration options.</param>
    /// <param name="connectionString">Connection string to Postgres DB.</param>
    /// <returns>Configuration options.</returns>
    public static EventSourcingOptions UsePostgresEventsStore(this EventSourcingOptions options, string connectionString)
    {
        options._services.Remove(typeof(IResolveAppender));
        options._services.Remove(typeof(IAppendOnly));
        options._services.AddTransient<IResolveAppender>(x =>
        {
            IPayloadSerializer payloadSerializer = x.GetRequiredService<IPayloadSerializer>();
            return new PgAppenderResolver(connectionString, payloadSerializer);
        });
        return options;
    }

    /// <summary>
    /// Use Postgres as snapshots store.
    /// </summary>
    /// <param name="options">Configuration options.</param>
    /// <param name="connectionString">Connection string to Postgres DB.</param>
    /// <returns>Configuration options.</returns>
    public static EventSourcingOptions UsePostgresSnapshotStore(
        this EventSourcingOptions options,
        string connectionString)
    {
        options._services.Remove(typeof(IResolveSnapshotStore));
        options._services.Remove(typeof(ISnapshotStore));

        options._services.AddTransient<IResolveSnapshotStore>(x =>
        {
            IPayloadSerializer payloadSerializer = x.GetRequiredService<IPayloadSerializer>();
            return new PgSnapshotStoreResolver(connectionString, payloadSerializer);
        });
        return options;
    }
}