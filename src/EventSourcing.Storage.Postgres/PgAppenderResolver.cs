using EventSourcing.Abstractions;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using Npgsql;

namespace EventSourcing.Storage.Postgres;

/// <inheritdoc />
public sealed class PgAppenderResolver : IResolveAppender
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly IPayloadSerializer _serializer;
    private readonly IPgCommandTextProvider _commandTextProvider;
    private readonly IPgCommandsBuilder _commandsBuilder;
    private readonly PgStorageOptions _storageOptions;

    public PgAppenderResolver(
        string connectionString,
        IPayloadSerializer serializer,
        IPgCommandTextProvider commandTextProvider,
        IPgCommandsBuilder commandsBuilder,
        PgStorageOptions storageOptions)
    {
        _commandsBuilder = commandsBuilder;
        _storageOptions = storageOptions;
        _commandTextProvider = commandTextProvider;
        _serializer = serializer;

        NpgsqlDataSourceBuilder builder = new NpgsqlDataSourceBuilder(connectionString);
        builder.UseNodaTime();
        _dataSource = builder.Build();
    }
        
    public IAppendOnly Get(TenantId tenantId)
    {
        return new PgSqlAppender(_serializer, _dataSource,  _commandTextProvider, _commandsBuilder, _storageOptions, tenantId);
    }
}