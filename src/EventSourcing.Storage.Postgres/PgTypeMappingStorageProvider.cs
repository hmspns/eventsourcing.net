using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Types;
using Npgsql;

namespace EventSourcing.Storage.Postgres;

public class PgTypeMappingStorageProvider : ITypeMappingStorageProvider
{
    private readonly PgStorageOptions _options;
    private readonly IPgCommandsBuilder _commandsBuilder;
    private readonly NpgsqlDataSource _dataSource;

    public PgTypeMappingStorageProvider(string connectionString, PgStorageOptions options,
        IPgCommandsBuilder commandsBuilder)
    {
        NpgsqlDataSourceBuilder builder = new NpgsqlDataSourceBuilder(connectionString);
        builder.UseNodaTime();
        _dataSource = builder.Build();
        _commandsBuilder = commandsBuilder;
        _options = options;
    }
    
    public async Task Initialize()
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();
        await using NpgsqlCommand cmd = _commandsBuilder.GetCreateTypeMappingStorageCommand(_options.MetadataSchemaName, _options.TypeMappingsTableName);
        cmd.Connection = connection;

        await connection.OpenAsync().ConfigureAwait(false);
        
        await cmd.ExecuteNonQueryAsync();

        await connection.CloseAsync().ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<TypeMapping>> GetMappings()
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();
        await using NpgsqlCommand cmd = _commandsBuilder.GetSelectTypeMappingsCommand(_options.MetadataSchemaName, _options.TypeMappingsTableName);
        cmd.Connection = connection;

        await connection.OpenAsync().ConfigureAwait(false);

        List<TypeMapping> mappings = new List<TypeMapping>();
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            Guid id = reader.GetGuid(0);
            string typeName = reader.GetString(1);
            
            mappings.Add(new TypeMapping(id, typeName));
        }

        await reader.CloseAsync().ConfigureAwait(false);
        await connection.CloseAsync().ConfigureAwait(false);

        return mappings;
    }

    public async Task AddMappings(IEnumerable<TypeMapping> mappings)
    {
        await using NpgsqlBatch batch = _dataSource.CreateBatch();
        foreach (TypeMapping mapping in mappings)
        {
            NpgsqlBatchCommand cmd = _commandsBuilder.GetInsertTypeMappingCommand(
                mapping.Id,
                mapping.TypeName,
                _options.MetadataSchemaName,
                _options.TypeMappingsTableName);
            batch.BatchCommands.Add(cmd);
        }

        await batch.ExecuteNonQueryAsync().ConfigureAwait(false);
    }
}