namespace EventSourcing.Net.Storage.SQLite.Commands;

using System.Data.Common;
using Abstractions.Contracts;
using Abstractions.Identities;
using Abstractions.Types;
using Microsoft.Data.Sqlite;

public interface ISqliteCommandsBuilder
{
    SqliteCommand GetInsertEventCommand<TId>(ref InsertEventCommandArguments<TId> insertEventCommandArguments);
    
    SqliteCommand GetInsertCommandCommand<TId>(IAppendDataPackage<TId> data,
                                                             byte[] payload,
                                                             TypeMappingId payloadType,
                                                             string schemaName,
                                                             string commandsTableName);

    SqliteCommand GetStreamVersionCommand(
        StreamId streamName,
        string schemaName,
        string eventsTableName);

    SqliteCommand GetEventsStreamCountCommand(string schemaName, string tableName);

    SqliteCommand GetSelectEventsDataCommand(
        StreamId streamName,
        StreamPosition from,
        StreamPosition to,
        string schemaName,
        string eventsTableName);

    SqliteCommand GetFindStreamIdsByPatternCommand(
        string startsWithPrefix,
        string schemaName,
        string eventsTableName);

    SqliteCommand GetCreateStorageCommand(
        string schemaName,
        string eventsTableName,
        string commandsTableName);

    SqliteCommand GetCheckStorageExistsCommand(
        string schemaName,
        string eventsTableName);

    SqliteCommand GetReadAllStreamsCommand(
        StreamReadOptions readOptions,
        string schemaName,
        string eventsTableName);

    SqliteCommand GetCreateTypeMappingStorageCommand(string schemaName, string tableName);
    SqliteCommand GetSelectTypeMappingsCommand(string schemaName, string tableName);

    SqliteCommand GetInsertTypeMappingCommand(
        TypeMappingId id,
        string name,
        string schemaName,
        string tableName);
}