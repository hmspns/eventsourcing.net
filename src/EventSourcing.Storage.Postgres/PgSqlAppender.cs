using System.Text;
using EventSourcing.Abstractions;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Abstractions.Types;
using EventSourcing.Core;
using EventSourcing.Core.Exceptions;
using Npgsql;
using NpgsqlTypes;
using IsolationLevel = System.Data.IsolationLevel;

namespace EventSourcing.Storage.Postgres;

/// <inheritdoc />
internal sealed class PgSqlAppender : PgSqlAppenderBase
{
    /// <summary>
    /// Initialize new object.
    /// </summary>
    /// <param name="serializer">Payload serializer.</param>
    /// <param name="connectionString">Connection string.</param>
    /// <param name="schemaName">Schema name.</param>
    public PgSqlAppender(IPayloadSerializer serializer, NpgsqlDataSource dataSource, string schemaName) 
        : base(serializer, dataSource, schemaName)
    {

    }

    protected override NpgsqlBatchCommand GetInsertEventCommand(IAppendDataPackage data, IAppendEventPackage appendPackage, long position, object payload)
    {
        var serialized = Serialize(payload, out string type);
        NpgsqlBatchCommand cmd = new NpgsqlBatchCommand(string.Format(APPEND_EVENT_COMMAND, _schemaName, EVENTS_TABLE_NAME));
        cmd.AddParameter(appendPackage.EventId.Id);
        cmd.AddParameter(data.CommandPackage.TenantId.Id);
        cmd.AddParameter(appendPackage.StreamName.ToString());
        cmd.AddParameter(position);
        cmd.AddParameter(appendPackage.Timestamp);
        cmd.AddParameter(data.CommandPackage.CommandId.Id);
        cmd.AddParameter(data.CommandPackage.SequenceId.Id);
        cmd.AddParameter(data.CommandPackage.PrincipalId.ToString());
        cmd.AddParameter(type);
        cmd.AddJsonParameter(serialized);
        return cmd;
    }

    protected override NpgsqlBatchCommand GetInsertCommandCommand(IAppendDataPackage data, object payload)
    {
        var serialized = Serialize(payload, out string type);
        NpgsqlBatchCommand cmd = new NpgsqlBatchCommand(string.Format(APPEND_COMMAND_COMMAND, _schemaName, COMMANDS_TABLE_NAME));
        cmd.AddParameter(data.CommandPackage.CommandId.Id);
        cmd.AddParameter(data.CommandPackage.ParentCommandId.Id);
        cmd.AddParameter(data.CommandPackage.SequenceId.Id);
        cmd.AddParameter(data.CommandPackage.TenantId.Id);
        cmd.AddParameter(data.CommandPackage.Timestamp);
        cmd.AddParameter(data.CommandPackage.AggregateId.ToString());
        cmd.AddParameter(data.CommandPackage.PrincipalId.ToString());
        cmd.AddParameter(data.CommandPackage.Source);
        cmd.AddParameter(type);
        cmd.AddJsonParameter(serialized);
        return cmd;
    }
}