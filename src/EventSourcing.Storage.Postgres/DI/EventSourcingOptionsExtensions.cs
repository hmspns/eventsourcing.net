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
    public static PostgresOptions UsePostgresEventsStore(this EventSourcingOptions options, string connectionString)
    {
        if (connectionString == null)
        {
            throw new ArgumentNullException(nameof(connectionString));
        }
        
        options._services.Remove<IResolveAppender>();
        options._services.Remove<IAppendOnly>();
        options._services.AddSingleton<IResolveAppender>(x =>
        {
            IEventsPayloadSerializerFactory serializerFactory = x.GetRequiredService<IEventsPayloadSerializerFactory>();
            IPayloadSerializer payloadSerializer = serializerFactory.GetSerializer();
            IPgCommandsBuilder commandsBuilder = x.GetRequiredService<IPgCommandsBuilder>();
            PgStorageOptions storageOptions = x.GetRequiredService<PgStorageOptions>();
            
            return new PgAppenderResolver(
                connectionString,
                payloadSerializer,
                commandsBuilder,
                storageOptions);
        });
        return new PostgresOptions(options._services);
    }

    // /// <summary>
    // /// Use Postgres as snapshots store.
    // /// </summary>
    // /// <param name="options">Configuration options.</param>
    // /// <param name="connectionString">Connection string to Postgres DB.</param>
    // /// <returns>Configuration options.</returns>
    // public static EventSourcingOptions UsePostgresSnapshotStore(
    //     this EventSourcingOptions options,
    //     string connectionString)
    // {
    //     if (connectionString == null)
    //     {
    //         throw new ArgumentNullException(nameof(connectionString));
    //     }
    //     options._services.Remove<IResolveEventStore>();
    //     options._services.Remove<ISnapshotStore>();
    //
    //     options._services.AddTransient<IResolveSnapshotStore>(x =>
    //     {
    //         IPayloadSerializer payloadSerializer = x.GetRequiredService<IPayloadSerializer>();
    //         return new PgSnapshotStoreResolver(connectionString, payloadSerializer);
    //     });
    //     return options;
    // }
}