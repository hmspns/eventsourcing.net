using EventSourcing.Net.Abstractions.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net.Storage.Postgres;

public static class EventSourcingOptionsExtensions
{
    /// <summary>
    /// Use Postgres as events store.
    /// </summary>
    /// <param name="options">Configuration options.</param>
    /// <param name="connectionString">Connection string to Postgres DB.</param>
    /// <param name="configurator">Optional callback to configure postgres store.</param>
    /// <returns>Configuration options.</returns>
    public static PostgresOptions UsePostgresEventStore(
    this EventSourcingStorageOptions options,
    string connectionString,
    Action<PgStorageOptions>? configurator = null)
    {
        if (connectionString == null)
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        PgStorageOptions storageOptions = new PgStorageOptions();
        configurator?.Invoke(storageOptions);

        options
            .ReplaceSingleton<IPgCommandTextProvider, PgCommandTextProvider>()
            .ReplaceSingleton<IPgCommandsBuilder, PgCommandsBuilder>()
            .ReplaceSingleton<PgStorageOptions>(storageOptions)
            
            .ReplaceSingleton<IResolveAppender>(x =>
            {
                IPayloadSerializerFactory serializerFactory = x.GetRequiredService<IPayloadSerializerFactory>();
                IPayloadSerializer payloadSerializer = serializerFactory.GetSerializer();
                IPgCommandsBuilder commandsBuilder = x.GetRequiredService<IPgCommandsBuilder>();
                ITypeMappingHandler typeMappingHandler = x.GetRequiredService<ITypeMappingHandler>();

                return new PgAppenderResolver(
                connectionString,
                payloadSerializer,
                commandsBuilder,
                typeMappingHandler,
                storageOptions);
            })
            
            .ReplaceTransient<ITypeMappingStorageProvider>(x =>
            {
                IPgCommandsBuilder commandsBuilder = x.GetRequiredService<IPgCommandsBuilder>();
                return new PgTypeMappingStorageProvider(connectionString, storageOptions, commandsBuilder);
            })
            .ReplaceTransient<IEventSourcingStorage, PgEventSourcingStorage>();
        
        return new PostgresOptions(storageOptions);
    }
    
    /// <summary>
    /// Use Postgres as events store.
    /// </summary>
    /// <param name="options">Configuration options.</param>
    /// <param name="connectionString">Connection string to Postgres DB.</param>
    /// <param name="configurator">Optional callback to configure postgres store.</param>
    /// <returns>Configuration options.</returns>
    [Obsolete("Use EventSourcingOptions.Storage.UsePostgresEventStore")]
    public static PostgresOptions UsePostgresEventsStore(
        this EventSourcingOptions options,
        string connectionString, 
        Action<PgStorageOptions>? configurator = null)
    {
        return options.Storage.UsePostgresEventStore(connectionString, configurator);
    }
}