using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using Npgsql;

namespace EventSourcing.Storage.Postgres;

public interface IPgCommandsBuilder
{
    NpgsqlBatchCommand GetInsertEventCommand(
        IAppendDataPackage data,
        IAppendEventPackage appendPackage,
        long position,
        byte[] payload,
        string payloadType,
        string schemaName,
        string tableName);

    NpgsqlBatchCommand GetInsertCommandCommand(
        IAppendDataPackage data,
        byte[] payload,
        string payloadType,
        string schemaName,
        string tableName);

    NpgsqlCommand GetVersionCommand(
        StreamId streamName,
        string schemaName,
        string tableName);
}