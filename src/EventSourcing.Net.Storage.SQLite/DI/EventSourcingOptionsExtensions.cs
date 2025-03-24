namespace EventSourcing.Net.Storage.SQLite.DI;

using Commands;
using Abstractions.Contracts;
using Microsoft.Extensions.DependencyInjection;

public static class EventSourcingOptionsExtensions
{
    /// <summary>
    /// Use Postgres as events store.
    /// </summary>
    /// <param name="options">Configuration options.</param>
    /// <param name="connectionString">Connection string to Postgres DB.</param>
    /// <param name="configurator">Optional callback to configure postgres store.</param>
    /// <returns>Configuration options.</returns>
    public static SqliteOptions UseSqliteEventStore(
        this EventSourcingStorageOptions options,
        string connectionString,
    Action<SqliteStorageOptions>? configurator = null)
    {
        if (connectionString == null)
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        SqliteStorageOptions storageOptions = new SqliteStorageOptions();
        configurator?.Invoke(storageOptions);

        options
            .ReplaceSingleton<ISqliteCommandTextProvider, SqliteCommandTextProvider>()
            .ReplaceSingleton<ISqliteCommandsBuilder, SqliteCommandsBuilder>()
            .ReplaceSingleton<SqliteStorageOptions>(storageOptions)
            
            .ReplaceSingleton<IResolveAppender>(x =>
            {
                IPayloadSerializerFactory serializerFactory = x.GetRequiredService<IPayloadSerializerFactory>();
                IPayloadSerializer payloadSerializer = serializerFactory.GetSerializer();
                ISqliteCommandsBuilder commandsBuilder = x.GetRequiredService<ISqliteCommandsBuilder>();
                ITypeMappingHandler typeMappingHandler = x.GetRequiredService<ITypeMappingHandler>();

                return new SqliteAppenderResolver(
                connectionString,
                payloadSerializer,
                commandsBuilder,
                typeMappingHandler,
                storageOptions);
            })
            
            .ReplaceTransient<ITypeMappingStorageProvider>(x =>
            {
                ISqliteCommandsBuilder commandsBuilder = x.GetRequiredService<ISqliteCommandsBuilder>();
                return new SqliteTypeMappingStorageProvider(connectionString, storageOptions, commandsBuilder);
            })
            .ReplaceTransient<IEventSourcingStorage, SqliteEventSourcingStorage>();
        
        return new SqliteOptions(storageOptions);
    }
}