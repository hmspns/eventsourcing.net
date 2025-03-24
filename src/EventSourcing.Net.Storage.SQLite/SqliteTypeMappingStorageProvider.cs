namespace EventSourcing.Net.Storage.SQLite;

using System.Data.Common;
using Commands;
using Abstractions.Contracts;
using Abstractions.Types;
using Engine.Pooled.Collections;
using Microsoft.Data.Sqlite;

public class SqliteTypeMappingStorageProvider : ITypeMappingStorageProvider
{
    private readonly SqliteStorageOptions _options;
    private readonly ISqliteCommandsBuilder _commandsBuilder;
    private readonly SqliteDataSource _dataSource;

    public SqliteTypeMappingStorageProvider(
        SqliteDataSource dataSource,
        SqliteStorageOptions options,
        ISqliteCommandsBuilder commandsBuilder)
    {
        _dataSource = dataSource;
        _commandsBuilder = commandsBuilder;
        _options = options;
    }

    public Task Initialize()
    {
        using SqliteConnection connection = _dataSource.OpenConnection();
        using SqliteCommand cmd = _commandsBuilder.GetCreateTypeMappingStorageCommand(_options.MetadataSchemaName, _options.TypeMappingsTableName);
        cmd.Connection = connection;

        connection.Open();

        cmd.ExecuteNonQuery();

        connection.Close();

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<TypeMapping>> GetMappings()
    {
        using SqliteConnection connection = _dataSource.OpenConnection();
        using SqliteCommand cmd = _commandsBuilder.GetSelectTypeMappingsCommand(_options.MetadataSchemaName, _options.TypeMappingsTableName);
        cmd.Connection = connection;

        connection.Open();

        List<TypeMapping> mappings = new List<TypeMapping>();
        using SqliteDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Guid id = reader.GetGuid(0);
            string typeName = reader.GetString(1);

            mappings.Add(new TypeMapping(id, typeName));
        }

        reader.Close();
        connection.Close();

        return Task.FromResult((IReadOnlyCollection<TypeMapping>)mappings);
    }

    public Task AddMappings(IEnumerable<TypeMapping> mappings)
    {
        using SqliteConnection connection = _dataSource.OpenConnection();
        using SqliteTransaction transaction = connection.BeginTransaction();

        foreach (TypeMapping mapping in mappings)
        {
            SqliteCommand cmd = _commandsBuilder.GetInsertTypeMappingCommand(
                mapping.Id,
                mapping.TypeName,
                _options.MetadataSchemaName,
                _options.TypeMappingsTableName);
            cmd.ExecuteNonQuery();
        }

        transaction.Commit();

        return Task.CompletedTask;
    }
}