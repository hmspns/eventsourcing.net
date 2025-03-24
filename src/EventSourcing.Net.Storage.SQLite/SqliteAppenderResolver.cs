namespace EventSourcing.Net.Storage.SQLite;

using Commands;
using Abstractions.Contracts;
using Abstractions.Identities;

/// <inheritdoc />
public sealed class SqliteAppenderResolver : IResolveAppender
{
    private readonly SqliteDataSource _dataSource;
    private readonly IPayloadSerializer _serializer;
    private readonly ISqliteCommandsBuilder _commandsBuilder;
    private readonly SqliteStorageOptions _storageOptions;
    private readonly ITypeMappingHandler _typeMappingHandler;

    public SqliteAppenderResolver(
        string connectionString,
        IPayloadSerializer serializer,
        ISqliteCommandsBuilder commandsBuilder,
        ITypeMappingHandler typeMappingHandler,
        SqliteStorageOptions storageOptions)
    {
        _typeMappingHandler = typeMappingHandler;
        _commandsBuilder = commandsBuilder;
        _storageOptions = storageOptions;
        _serializer = serializer;

        _dataSource = new SqliteDataSource(connectionString);
    }
        
    public IAppendOnly Get(TenantId tenantId)
    {
        return new SqliteSqlAppender(_serializer, _dataSource, _commandsBuilder, _storageOptions, _typeMappingHandler, tenantId);
    }
}