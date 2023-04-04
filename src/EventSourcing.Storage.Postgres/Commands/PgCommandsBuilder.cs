using System.Runtime.CompilerServices;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Abstractions.Types;
using Npgsql;

namespace EventSourcing.Storage.Postgres;

public sealed class PgCommandsBuilder : IPgCommandsBuilder
{
    private readonly PgStorageOptions _storageOptions;
    private readonly IPgCommandTextProvider _commandTextProvider;

    public PgCommandsBuilder(PgStorageOptions storageOptions, IPgCommandTextProvider commandTextProvider)
    {
        _commandTextProvider = commandTextProvider;
        _storageOptions = storageOptions;
    }

    public NpgsqlBatchCommand GetInsertEventCommand(
        IAppendDataPackage data,
        IAppendEventPackage appendPackage,
        long position,
        byte[] payload,
        string payloadType,
        string schemaName,
        string eventsTableName)
    {
        NpgsqlBatchCommand cmd = new NpgsqlBatchCommand(string.Format(_commandTextProvider.InsertEvent, schemaName, eventsTableName));
        cmd.AddParameter(appendPackage.EventId.Id);
        cmd.AddParameter(appendPackage.StreamName.ToString());
        cmd.AddParameter(position);
        cmd.AddParameter(appendPackage.Timestamp);
        cmd.AddParameter(data.CommandPackage.CommandId.Id);
        cmd.AddParameter(data.CommandPackage.SequenceId.Id);
        cmd.AddParameter(payloadType);
        cmd.AddJsonParameter(payload);
        
        if (_storageOptions.UseMultitenancy)
        {
            cmd.AddParameter(data.CommandPackage.TenantId.Id);
        }

        if (_storageOptions.StorePrincipal)
        {
            cmd.AddParameter(data.CommandPackage.PrincipalId.ToString());
        }

        return cmd;
    }

    public NpgsqlBatchCommand GetInsertCommandCommand(
        IAppendDataPackage data,
        byte[] payload,
        string payloadType,
        string schemaName,
        string commandsTableName)
    {
        NpgsqlBatchCommand cmd = new NpgsqlBatchCommand(string.Format(_commandTextProvider.InsertCommand, schemaName, commandsTableName));
        cmd.AddParameter(data.CommandPackage.CommandId.Id);
        cmd.AddParameter(data.CommandPackage.ParentCommandId.Id);
        cmd.AddParameter(data.CommandPackage.SequenceId.Id);
        cmd.AddParameter(data.CommandPackage.Timestamp);
        cmd.AddParameter(data.CommandPackage.AggregateId.ToString());
        cmd.AddParameter(payloadType);
        cmd.AddJsonParameter(payload);
        if (_storageOptions.UseMultitenancy)
        {
            cmd.AddParameter(data.CommandPackage.TenantId.Id);
        }
        if (_storageOptions.StorePrincipal)
        {
            cmd.AddParameter(data.CommandPackage.PrincipalId.ToString());
        }
        if (_storageOptions.StoreCommandSource)
        {
            cmd.AddParameter(data.CommandPackage.Source);
        }
        return cmd;
    }

    public NpgsqlCommand GetStreamVersionCommand(
        StreamId streamName,
        string schemaName,
        string eventsTableName)
    {
        NpgsqlCommand selectVersionCommand = new NpgsqlCommand(
            string.Format(_commandTextProvider.SelectStreamVersion, schemaName, eventsTableName));
        selectVersionCommand.AddParameter(streamName.ToString());
                
        return selectVersionCommand;
    }
    
    public NpgsqlBatchCommand GetEventsStreamCountCommand(string schemaName, string tableName)
    {
        return new NpgsqlBatchCommand(string.Format(_commandTextProvider.SelectEventCounts, schemaName, tableName));
    }

    public NpgsqlBatchCommand GetSelectEventsDataCommand(
        StreamId streamName,
        StreamPosition from,
        StreamPosition to,
        string schemaName,
        string eventsTableName)
    {
        NpgsqlBatchCommand cmd = new NpgsqlBatchCommand(string.Format(_commandTextProvider.SelectStreamData, schemaName, eventsTableName));

        cmd.AddParameter(streamName.ToString());
        cmd.AddParameter(to - from);
        cmd.AddParameter(from);
        return cmd;
    }

    public NpgsqlCommand GetFindStreamIdsByPatternCommand(
        string startsWithPrefix,
        string schemaName,
        string eventsTableName)
    {
        NpgsqlCommand cmd = new NpgsqlCommand(string.Format(_commandTextProvider.SelectStreamIdsByPattern, schemaName, eventsTableName));

        cmd.AddParameter(startsWithPrefix + "%");
        return cmd;
    }

    public NpgsqlCommand GetCreateStorageCommand(
        string schemaName,
        string eventsTableName,
        string commandsTableName)
    {
        NpgsqlCommand cmd = new NpgsqlCommand(string.Format(
            _commandTextProvider.CreateStorage,
            schemaName,
            eventsTableName,
            commandsTableName));
        return cmd;
    }

    public NpgsqlCommand GetCheckStorageExistsCommand(
        string schemaName,
        string eventsTableName)
    {
        return new NpgsqlCommand(string.Format(_commandTextProvider.SelectStorageExists, schemaName, eventsTableName));
    }

    public NpgsqlCommand GetReadAllStreamsCommand(
        StreamReadOptions readOptions,
        string schemaName,
        string eventsTableName)
    {
        string commandText = _commandTextProvider.BuildReadAllStreamsCommandText(readOptions);
        NpgsqlCommand cmd = new NpgsqlCommand(string.Format(commandText, schemaName, eventsTableName));

        cmd.AddParameter(readOptions.To - readOptions.From);
        cmd.AddParameter(readOptions.From);

        return cmd;
    }
}