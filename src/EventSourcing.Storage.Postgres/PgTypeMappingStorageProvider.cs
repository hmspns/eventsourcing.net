using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Types;
using EventSourcing.Core;
using Npgsql;

namespace EventSourcing.Storage.Postgres;

public class PgTypeMappingStorageProvider : ITypeMappingStorageProvider
{
    private readonly PgStorageOptions _options;
    private readonly IPgCommandsBuilder _commandsBuilder;
    private readonly NpgsqlDataSource _dataSource;

    public PgTypeMappingStorageProvider(
        PgStorageOptions options,
        IPgCommandsBuilder commandsBuilder,
        NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
        _commandsBuilder = commandsBuilder;
        _options = options;
    }
    
    public async Task Initialize()
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();
        await using NpgsqlCommand cmd = _commandsBuilder.GetCreateTypeMappingStorageCommand(_options.MetadataSchemaName, _options.TypeMappingsTableName);
        cmd.Connection = connection;

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyCollection<TypeMapping>> GetMappings()
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();
        await using NpgsqlCommand cmd = _commandsBuilder.GetSelectTypeMappingsCommand(_options.MetadataSchemaName, _options.TypeMappingsTableName);
        cmd.Connection = connection;

        List<TypeMapping> mappings = new List<TypeMapping>();
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            Guid id = reader.GetGuid(0);
            string typeName = reader.GetString(1);
            
            mappings.Add(new TypeMapping(id, typeName));
        }

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

        await batch.ExecuteNonQueryAsync();
    }
}