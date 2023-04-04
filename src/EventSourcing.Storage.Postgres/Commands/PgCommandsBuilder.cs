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
        string tableName)
    {
        NpgsqlBatchCommand cmd = new NpgsqlBatchCommand(string.Format(_commandTextProvider.InsertEvent, schemaName, tableName));
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
        string tableName)
    {
        NpgsqlBatchCommand cmd = new NpgsqlBatchCommand(string.Format(_commandTextProvider.InsertCommand, schemaName, tableName));
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NpgsqlCommand GetVersionCommand(
        StreamId streamName,
        string schemaName,
        string tableName)
    {
        NpgsqlCommand selectVersionCommand = new NpgsqlCommand(
            string.Format(_commandTextProvider.SelectStreamVersion, schemaName, tableName));
        selectVersionCommand.AddParameter(streamName.ToString());
                
        return selectVersionCommand;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NpgsqlBatchCommand GetEventsStreamCountCommand(string schemaName, string tableName)
    {
        return new NpgsqlBatchCommand(string.Format(_commandTextProvider.SelectEventCounts, schemaName, tableName));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NpgsqlBatchCommand GetSelectEventsDataCommand(
        StreamId streamName,
        StreamPosition from,
        StreamPosition to,
        string schemaName,
        string tableName)
    {
        NpgsqlBatchCommand cmd = new NpgsqlBatchCommand(string.Format(_commandTextProvider.SelectStreamData, schemaName, tableName));

        cmd.AddParameter(streamName.ToString());
        cmd.AddParameter(to - from);
        cmd.AddParameter(from);
        return cmd;
    }
}