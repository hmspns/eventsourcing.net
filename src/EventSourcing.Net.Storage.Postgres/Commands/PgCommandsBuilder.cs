using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Abstractions.Types;
using Npgsql;

namespace EventSourcing.Net.Storage.Postgres;

using System.Runtime.CompilerServices;
using System.Text;

public sealed class PgCommandsBuilder : IPgCommandsBuilder
{
    private readonly PgStorageOptions _storageOptions;
    private readonly IPgCommandTextProvider _commandTextProvider;

    public PgCommandsBuilder(PgStorageOptions storageOptions, IPgCommandTextProvider commandTextProvider)
    {
        _commandTextProvider = commandTextProvider;
        _storageOptions = storageOptions;

#if NET8_0_OR_GREATER
        _insertEventFormat = CompositeFormat.Parse(commandTextProvider.InsertEvent);
        _insertCommandFormat = CompositeFormat.Parse(commandTextProvider.InsertCommand);
        _getStreamVersionFormat = CompositeFormat.Parse(commandTextProvider.SelectStreamVersion);
        _getEventsStreamCountFormat = CompositeFormat.Parse(commandTextProvider.SelectEventCounts);
        _getSelectEventsDataFormat = CompositeFormat.Parse(commandTextProvider.SelectStreamData);
        _getFindStreamIdsByPatternFormat = CompositeFormat.Parse(commandTextProvider.SelectStreamIdsByPattern);
        _getCreateStorageFormat = CompositeFormat.Parse(commandTextProvider.CreateDataStorage);
        _getCheckStorageExistsFormat = CompositeFormat.Parse(commandTextProvider.SelectStorageExists);
        _getCreateTypeMappingStorageFormat = CompositeFormat.Parse(commandTextProvider.CreateMappingsStorage);
        _getSelectTypeMappingsFormat = CompositeFormat.Parse(commandTextProvider.SelectTypeMappings);
        _insertTypeMappingFormat = CompositeFormat.Parse(commandTextProvider.InsertTypeMapping);
#endif
    }

    public NpgsqlBatchCommand GetInsertEventCommand<TId>(ref InsertEventCommandArguments<TId> insertEventCommandArguments)
    {
        NpgsqlBatchCommand cmd = new NpgsqlBatchCommand(FormatGetInsertEventCommand(insertEventCommandArguments));
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

    public NpgsqlBatchCommand GetInsertCommandCommand<TId>(
        IAppendDataPackage<TId> data,
        byte[] payload,
        TypeMappingId payloadType,
        string schemaName,
        string commandsTableName)
    {
        NpgsqlBatchCommand cmd = new NpgsqlBatchCommand(FormatGetInsertCommandCommand(schemaName, commandsTableName));
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
            FormatGetStreamVersionCommand(schemaName, eventsTableName));
        selectVersionCommand.AddParameter(streamName.ToString()!);

        return selectVersionCommand;
    }

    public NpgsqlBatchCommand GetEventsStreamCountCommand(string schemaName, string tableName)
    {
        return new NpgsqlBatchCommand(FormatGetEventsStreamCountCommand(schemaName, tableName));
    }

    public NpgsqlBatchCommand GetSelectEventsDataCommand(
        StreamId streamName,
        StreamPosition from,
        StreamPosition to,
        string schemaName,
        string eventsTableName)
    {
        NpgsqlBatchCommand cmd = new NpgsqlBatchCommand(FormatGetSelectEventsDataCommand(schemaName, eventsTableName));

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
        NpgsqlCommand cmd = new NpgsqlCommand(FormatGetFindStreamIdsByPatternCommand(schemaName, eventsTableName));

        cmd.AddParameter(startsWithPrefix + "%");
        return cmd;
    }

    public NpgsqlCommand GetCreateStorageCommand(
        string schemaName,
        string eventsTableName,
        string commandsTableName)
    {
        NpgsqlCommand cmd = new NpgsqlCommand(FormatGetCreateStorageCommand(schemaName, eventsTableName, commandsTableName));
        return cmd;
    }

    public NpgsqlCommand GetCheckStorageExistsCommand(
        string schemaName,
        string eventsTableName)
    {
        return new NpgsqlCommand(FormatGetCheckStorageExistsCommand(schemaName, eventsTableName));
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
        return new NpgsqlCommand(FormatGetCreateTypeMappingStorageCommand(schemaName, tableName));
    }

    public NpgsqlCommand GetSelectTypeMappingsCommand(string schemaName, string tableName)
    {
        return new NpgsqlCommand(FormatGetSelectTypeMappingsCommand(schemaName, tableName));
    }

    public NpgsqlBatchCommand GetInsertTypeMappingCommand(
        TypeMappingId id,
        string name,
        string schemaName,
        string tableName)
    {
        NpgsqlBatchCommand cmd = new NpgsqlBatchCommand(FormatGetInsertTypeMappingCommand(schemaName, tableName));

        cmd.AddParameter(id);
        cmd.AddParameter(name);

        return cmd;
    }

    #region Formatting

#if NET8_0_OR_GREATER
    private readonly CompositeFormat _insertEventFormat;
    private readonly CompositeFormat _insertCommandFormat;
    private readonly CompositeFormat _getStreamVersionFormat;
    private readonly CompositeFormat _getEventsStreamCountFormat;
    private readonly CompositeFormat _getSelectEventsDataFormat;
    private readonly CompositeFormat _getFindStreamIdsByPatternFormat;
    private readonly CompositeFormat _getCreateStorageFormat;
    private readonly CompositeFormat _getCheckStorageExistsFormat;
    private readonly CompositeFormat _getCreateTypeMappingStorageFormat;
    private readonly CompositeFormat _getSelectTypeMappingsFormat;
    private readonly CompositeFormat _insertTypeMappingFormat;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatGetInsertEventCommand<TId>(InsertEventCommandArguments<TId> insertEventCommandArguments)
    {
        return string.Format(null, _insertEventFormat, insertEventCommandArguments.SchemaName, insertEventCommandArguments.EventsTableName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatGetInsertCommandCommand(string schemaName, string commandsTableName)
    {
        return string.Format(null, _insertCommandFormat, schemaName, commandsTableName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatGetStreamVersionCommand(string schemaName, string eventsTableName)
    {
        return string.Format(null, _getStreamVersionFormat, schemaName, eventsTableName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatGetEventsStreamCountCommand(string schemaName, string tableName)
    {
        return string.Format(null, _getEventsStreamCountFormat, schemaName, tableName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatGetSelectEventsDataCommand(string schemaName, string eventsTableName)
    {
        return string.Format(null, _getSelectEventsDataFormat, schemaName, eventsTableName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatGetFindStreamIdsByPatternCommand(string schemaName, string eventsTableName)
    {
        return string.Format(null, _getFindStreamIdsByPatternFormat, schemaName, eventsTableName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatGetCreateStorageCommand(string schemaName, string eventsTableName, string commandsTableName)
    {
        return string.Format(
            null,
            _getCreateStorageFormat,
            schemaName,
            eventsTableName,
            commandsTableName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatGetCheckStorageExistsCommand(string schemaName, string eventsTableName)
    {
        return string.Format(null, _getCheckStorageExistsFormat, schemaName, eventsTableName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatGetCreateTypeMappingStorageCommand(string schemaName, string tableName)
    {
        return string.Format(null, _getCreateTypeMappingStorageFormat, schemaName, tableName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatGetSelectTypeMappingsCommand(string schemaName, string tableName)
    {
        return string.Format(null, _getSelectTypeMappingsFormat, schemaName, tableName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatGetInsertTypeMappingCommand(string schemaName, string tableName)
    {
        return string.Format(null, _insertTypeMappingFormat, schemaName, tableName);
    }
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatGetInsertEventCommand<TId>(InsertEventCommandArguments<TId> insertEventCommandArguments)
    {
        return string.Format(_commandTextProvider.InsertEvent, insertEventCommandArguments.SchemaName, insertEventCommandArguments.EventsTableName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatGetInsertCommandCommand(string schemaName, string commandsTableName)
    {
        return string.Format(_commandTextProvider.InsertCommand, schemaName, commandsTableName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatGetStreamVersionCommand(string schemaName, string eventsTableName)
    {
        return string.Format(_commandTextProvider.SelectStreamVersion, schemaName, eventsTableName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatGetEventsStreamCountCommand(string schemaName, string tableName)
    {
        return string.Format(_commandTextProvider.SelectEventCounts, schemaName, tableName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatGetSelectEventsDataCommand(string schemaName, string eventsTableName)
    {
        return string.Format(_commandTextProvider.SelectStreamData, schemaName, eventsTableName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatGetFindStreamIdsByPatternCommand(string schemaName, string eventsTableName)
    {
        return string.Format(_commandTextProvider.SelectStreamIdsByPattern, schemaName, eventsTableName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatGetCreateStorageCommand(string schemaName, string eventsTableName, string commandsTableName)
    {
        return string.Format(
            _commandTextProvider.CreateDataStorage,
            schemaName,
            eventsTableName,
            commandsTableName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatGetCheckStorageExistsCommand(string schemaName, string eventsTableName)
    {
        return string.Format(_commandTextProvider.SelectStorageExists, schemaName, eventsTableName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatGetCreateTypeMappingStorageCommand(string schemaName, string tableName)
    {
        return string.Format(_commandTextProvider.CreateMappingsStorage, schemaName, tableName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatGetSelectTypeMappingsCommand(string schemaName, string tableName)
    {
        return string.Format(_commandTextProvider.SelectTypeMappings, schemaName, tableName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatGetInsertTypeMappingCommand(string schemaName, string tableName)
    {
        return string.Format(_commandTextProvider.InsertTypeMapping, schemaName, tableName);
    }
#endif

    #endregion
}