using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Abstractions.Types;
using Npgsql;

namespace EventSourcing.Net.Storage.Postgres;

public sealed class PgCommandsBuilder : IPgCommandsBuilder
{
    private readonly PgStorageOptions _storageOptions;
    private readonly IPgCommandTextProvider _commandTextProvider;

    public PgCommandsBuilder(PgStorageOptions storageOptions, IPgCommandTextProvider commandTextProvider)
    {
        _commandTextProvider = commandTextProvider;
        _storageOptions = storageOptions;
    }

    public NpgsqlBatchCommand GetInsertEventCommand<TId>(
        ref InsertEventCommandArguments<TId> insertEventCommandArguments)
    {
        NpgsqlBatchCommand cmd = new NpgsqlBatchCommand(string.Format(_commandTextProvider.InsertEvent, insertEventCommandArguments.SchemaName, insertEventCommandArguments.EventsTableName));
        cmd.AddParameter(insertEventCommandArguments.AppendPackage.EventId.Id);
        cmd.AddParameter(insertEventCommandArguments.AppendPackage.StreamName.ToString());
        cmd.AddParameter(insertEventCommandArguments.AggregateIdType);
        cmd.AddParameter(insertEventCommandArguments.Position);
        cmd.AddParameter(insertEventCommandArguments.AppendPackage.Timestamp);
        cmd.AddParameter(insertEventCommandArguments.Data.CommandPackage.CommandId.Id);
        cmd.AddParameter(insertEventCommandArguments.Data.CommandPackage.SequenceId.Id);
        cmd.AddParameter(insertEventCommandArguments.PayloadType);
        cmd.AddBinaryParameter(insertEventCommandArguments.Payload, _storageOptions);
        
        if (_storageOptions.StoreTenantId)
        {
            cmd.AddParameter(insertEventCommandArguments.Data.CommandPackage.TenantId.Id);
        }

        if (_storageOptions.StorePrincipal)
        {
            cmd.AddParameter(insertEventCommandArguments.Data.CommandPackage.PrincipalId.ToString());
        }

        return cmd;
    }

    public NpgsqlBatchCommand GetInsertCommandCommand<TId>(IAppendDataPackage<TId> data,
        byte[] payload,
        TypeMappingId payloadType,
        string schemaName,
        string commandsTableName)
    {
        NpgsqlBatchCommand cmd = new NpgsqlBatchCommand(string.Format(_commandTextProvider.InsertCommand, schemaName, commandsTableName));
        cmd.AddParameter(data.CommandPackage.CommandId.Id);
        if (data.CommandPackage.ParentCommandId != CommandId.Empty)
        {
            cmd.AddParameter(data.CommandPackage.ParentCommandId.Id);
        }
        else
        {
            cmd.Parameters.Add(new NpgsqlParameter(null, DBNull.Value));
        }
        cmd.AddParameter(data.CommandPackage.SequenceId.Id);
        cmd.AddParameter(data.CommandPackage.Timestamp);
        cmd.AddParameter(data.CommandPackage.AggregateId.ToString());
        cmd.AddParameter(payloadType);
        cmd.AddBinaryParameter(payload, _storageOptions);
        if (_storageOptions.StoreTenantId)
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
            _commandTextProvider.CreateDataStorage,
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

    public NpgsqlCommand GetCreateTypeMappingStorageCommand(string schemaName, string tableName)
    {
        return new NpgsqlCommand(string.Format(_commandTextProvider.CreateMappingsStorage, schemaName, tableName));
    }

    public NpgsqlCommand GetSelectTypeMappingsCommand(string schemaName, string tableName)
    {
        return new NpgsqlCommand(string.Format(_commandTextProvider.SelectTypeMappings, schemaName, tableName));
    }

    public NpgsqlBatchCommand GetInsertTypeMappingCommand(
        TypeMappingId id,
        string name,
        string schemaName,
        string tableName)
    {
        NpgsqlBatchCommand cmd = new NpgsqlBatchCommand(string.Format(_commandTextProvider.InsertTypeMapping, schemaName, tableName));
        
        cmd.AddParameter(id);
        cmd.AddParameter(name);

        return cmd;
    }
}