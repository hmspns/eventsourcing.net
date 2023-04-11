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
    public static PostgresOptions UsePostgresEventsStore(
        this EventSourcingOptions options,
        string connectionString, 
        Action<PgStorageOptions>? configure = null)
    {
        if (connectionString == null)
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        PgStorageOptions storageOptions = new PgStorageOptions();
        configure?.Invoke(storageOptions);
        
        options.Services.AddSingleton<IPgCommandTextProvider, PgCommandTextProvider>();
        options.Services.AddSingleton<IPgCommandsBuilder, PgCommandsBuilder>();
        options.Services.AddSingleton<PgStorageOptions>(storageOptions);
        
        options.Services.Remove<IResolveAppender>();
        options.Services.Remove<IAppendOnly>();
        options.Services.AddSingleton<IResolveAppender>(x =>
        {
            IEventsPayloadSerializerFactory serializerFactory = x.GetRequiredService<IEventsPayloadSerializerFactory>();
            IPayloadSerializer payloadSerializer = serializerFactory.GetSerializer();
            IPgCommandsBuilder commandsBuilder = x.GetRequiredService<IPgCommandsBuilder>();
            ITypeMappingHandler typeMappingHandler = x.GetRequiredService<ITypeMappingHandler>();
            
            return new PgAppenderResolver(
                connectionString,
                payloadSerializer,
                commandsBuilder,
                typeMappingHandler,
                storageOptions);
        });
        options.Services.AddTransient<ITypeMappingStorageProvider>(x =>
        {
            IPgCommandsBuilder commandsBuilder = x.GetRequiredService<IPgCommandsBuilder>();
            return new PgTypeMappingStorageProvider(connectionString, storageOptions, commandsBuilder);
        });
        options.Services.AddTransient<IEventSourcingStorage, PgEventSourcingStorage>();
        return new PostgresOptions(storageOptions);
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
    //     options.Services.Remove<IResolveEventStore>();
    //     options.Services.Remove<ISnapshotStore>();
    //
    //     options.Services.AddTransient<IResolveSnapshotStore>(x =>
    //     {
    //         IPayloadSerializer payloadSerializer = x.GetRequiredService<IPayloadSerializer>();
    //         return new PgSnapshotStoreResolver(connectionString, payloadSerializer);
    //     });
    //     return options;
    // }
}