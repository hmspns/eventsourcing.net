using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Abstractions.Types;
using Npgsql;

namespace EventSourcing.Storage.Postgres;

public interface IPgCommandsBuilder
{
    NpgsqlBatchCommand GetInsertEventCommand<TId>(ref InsertEventCommandArguments<TId> insertEventCommandArguments);

    NpgsqlBatchCommand GetInsertCommandCommand<TId>(IAppendDataPackage<TId> data,
        byte[] payload,
        TypeMappingId payloadType,
        string schemaName,
        string commandsTableName);

    NpgsqlCommand GetStreamVersionCommand(
        StreamId streamName,
        string schemaName,
        string eventsTableName);

    NpgsqlBatchCommand GetEventsStreamCountCommand(string schemaName, string tableName);

    NpgsqlBatchCommand GetSelectEventsDataCommand(
        StreamId streamName,
        StreamPosition from,
        StreamPosition to,
        string schemaName,
        string eventsTableName);

    NpgsqlCommand GetFindStreamIdsByPatternCommand(
        string startsWithPrefix,
        string schemaName,
        string eventsTableName);

    NpgsqlCommand GetCreateStorageCommand(
        string schemaName,
        string eventsTableName,
        string commandsTableName);

    NpgsqlCommand GetCheckStorageExistsCommand(
        string schemaName,
        string eventsTableName);

    NpgsqlCommand GetReadAllStreamsCommand(
        StreamReadOptions readOptions,
        string schemaName,
        string eventsTableName);

    NpgsqlCommand GetCreateTypeMappingStorageCommand(string schemaName, string tableName);
    NpgsqlCommand GetSelectTypeMappingsCommand(string schemaName, string tableName);

    NpgsqlBatchCommand GetInsertTypeMappingCommand(
        TypeMappingId id,
        string name,
        string schemaName,
        string tableName);
}