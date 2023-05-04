using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using Npgsql;

namespace EventSourcing.Net.Storage.Postgres;

/// <inheritdoc />
public sealed class PgAppenderResolver : IResolveAppender
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly IPayloadSerializer _serializer;
    private readonly IPgCommandsBuilder _commandsBuilder;
    private readonly PgStorageOptions _storageOptions;
    private readonly ITypeMappingHandler _typeMappingHandler;

    public PgAppenderResolver(
        string connectionString,
        IPayloadSerializer serializer,
        IPgCommandsBuilder commandsBuilder,
        ITypeMappingHandler typeMappingHandler,
        PgStorageOptions storageOptions)
    {
        _typeMappingHandler = typeMappingHandler;
        _commandsBuilder = commandsBuilder;
        _storageOptions = storageOptions;
        _serializer = serializer;

        NpgsqlDataSourceBuilder builder = new NpgsqlDataSourceBuilder(connectionString);
        builder.UseNodaTime();
        _dataSource = builder.Build();
    }
        
    public IAppendOnly Get(TenantId tenantId)
    {
        return new PgSqlAppender(_serializer, _dataSource, _commandsBuilder, _storageOptions, _typeMappingHandler, tenantId);
    }
}