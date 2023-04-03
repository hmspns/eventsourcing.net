using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using Npgsql;

namespace EventSourcing.Storage.Postgres;

public class PgCommandsBuilder : IPgCommandsBuilder
{
    protected readonly PgStorageOptions _storageOptions;
    protected readonly PgCommandTextProvider CommandTextProvider;

    public PgCommandsBuilder(PgStorageOptions storageOptions, PgCommandTextProvider commandTextProvider)
    {
        CommandTextProvider = commandTextProvider;
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
        NpgsqlBatchCommand cmd = new NpgsqlBatchCommand(string.Format(CommandTextProvider.InsertEvent, schemaName, tableName));
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
        NpgsqlBatchCommand cmd = new NpgsqlBatchCommand(string.Format(CommandTextProvider.InsertCommand, schemaName, tableName));
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

    public NpgsqlCommand GetVersionCommand(
        StreamId streamName,
        string schemaName,
        string tableName)
    {
        NpgsqlCommand selectVersionCommand = new NpgsqlCommand(
            string.Format(CommandTextProvider.SelectStreamVersion, schemaName, tableName));
        selectVersionCommand.AddParameter(streamName.ToString());
                
        return selectVersionCommand;
    }
}