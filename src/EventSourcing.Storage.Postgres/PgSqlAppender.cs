using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Abstractions.Types;
using EventSourcing.Core;
using EventSourcing.Core.Exceptions;
using Npgsql;

namespace EventSourcing.Storage.Postgres;

public sealed class PgSqlAppender : IAppendOnly
{
    protected const string INITIAL_COMMAND =
        @"
CREATE SCHEMA IF NOT EXISTS ""{0}"";

CREATE TABLE IF NOT EXISTS ""{0}"".""{1}""
(
    id              uuid
        constraint ""{1}_pk""
            primary key,
    tenant_id       uuid         not null,
    stream_name     varchar(255) not null,
    stream_position bigint       not null,
    global_position bigint GENERATED ALWAYS AS identity,
    timestamp       timestamptz  not null,
    command_id      uuid         not null,
    sequence_id     uuid         not null,
    principal_id    varchar(255) not null,
    payload_type    varchar(255) not null,
    payload         jsonb        not null
);

CREATE UNIQUE INDEX IF NOT EXISTS ""{1}__version""
    ON ""{0}"".""{1}"" (stream_name, stream_position);

CREATE TABLE IF NOT EXISTS ""{0}"".""{2}""
(
    id                uuid         not null
        constraint ""{2}_pk""
            primary key,
    parent_command_id uuid         not null,
    sequence_id       uuid         not null,
    tenant_id         uuid         not null,
    timestamp         timestamptz  not null,
    aggregate_id      varchar(255) not null,
    principal_id      varchar(255) not null,
    command_source    varchar(255) not null,
    payload_type      varchar(255) not null,
    payload           jsonb        not null
);
";

    private const string EXIST_COMMAND =
        @"SELECT EXISTS (
    SELECT FROM information_schema.tables 
    WHERE table_schema = '{0}' AND table_name = '{1}'
);";

    protected const string SEARCH_FOR_STREAM_ID =
        @"SELECT stream_name FROM ""{0}"".""{1}"" 
              WHERE stream_name LIKE $1";

    private readonly IPayloadSerializer _serializer;
    private readonly NpgsqlDataSource _dataSource;
    private readonly IPgCommandsBuilder _commandsBuilder;
    private readonly PgStorageOptions _storageOptions;
    private readonly TenantId _tenantId;
    
    private readonly string _eventsTableName;
    private readonly string _commandsTableName;

    private string SchemaName
    {
        get
        {
            return _storageOptions.UseMultitenancy ?
                _storageOptions.MultitenancySchemaName(_tenantId) :
                _storageOptions.NonMultitenancySchemaName;
        }
    }

    internal PgSqlAppender(
        IPayloadSerializer serializer,
        NpgsqlDataSource dataSource,
        IPgCommandsBuilder commandsBuilder,
        PgStorageOptions storageOptions,
        TenantId tenantId)
    {
        _tenantId = tenantId;
        _commandsBuilder = commandsBuilder;
        _dataSource = dataSource;
        _serializer = serializer;
        _storageOptions = storageOptions;
        _eventsTableName = storageOptions.EventsTableName;
        _commandsTableName = storageOptions.CommandsTableName;
    }

    #region Dispose

    public void Dispose()
    {
        // Here is nothing to dispose.
    }

    public async ValueTask DisposeAsync()
    {
        // Here is nothing to dispose.
    }

    #endregion

    /// <summary>
    /// Append events to the events storage.
    /// </summary>
    /// <param name="streamName">Name of the stream.</param>
    /// <param name="data">Events data.</param>
    /// <param name="expectedStreamVersion">Expected version of events stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of append operation.</returns>
    /// <exception cref="AppendOnlyStoreConcurrencyException">Event with the given version already presents in the stream.</exception>
    public async Task<IAppendEventsResult> Append(
        StreamId streamName,
        IAppendDataPackage data,
        AggregateVersion expectedStreamVersion,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
            await using NpgsqlTransaction transaction = await conn.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            await using NpgsqlCommand selectVersionCommand = _commandsBuilder.GetVersionCommand(streamName, SchemaName, _eventsTableName);
            selectVersionCommand.Connection = conn;
            await selectVersionCommand.PrepareAsync(cancellationToken);

            // ReSharper disable once PossibleNullReferenceException
            long version = (long)await selectVersionCommand.ExecuteScalarAsync(cancellationToken);

            if (version != expectedStreamVersion)
            {
                throw new AppendOnlyStoreConcurrencyException(expectedStreamVersion, version, streamName.ToString());
            }

            await using NpgsqlBatch batch = new NpgsqlBatch(conn, transaction);
            long position = expectedStreamVersion;
            foreach (IAppendEventPackage appendPackage in data.EventPackages)
            {
                position += 1;
                byte[] eventPayload = Serialize(appendPackage.Payload, out string eventPayloadType);
                NpgsqlBatchCommand cmd = _commandsBuilder.GetInsertEventCommand(
                    data,
                    appendPackage,
                    position,
                    eventPayload,
                    eventPayloadType,
                    SchemaName, _eventsTableName);
                batch.BatchCommands.Add(cmd);
            }

            byte[] commandPayload = Serialize(data.CommandPackage.Payload, out string commandPayloadType);
            NpgsqlBatchCommand createCommandCmd = _commandsBuilder.GetInsertCommandCommand(
                data,
                commandPayload,
                commandPayloadType,
                SchemaName,
                _commandsTableName);
            batch.BatchCommands.Add(createCommandCmd);

            await batch.PrepareAsync(cancellationToken);

            await batch.ExecuteNonQueryAsync(cancellationToken);

            if (!cancellationToken.IsCancellationRequested)
            {
                await transaction.CommitAsync(cancellationToken);

                return new AppendEventsResult(true, position);
            }

            return new AppendEventsResult(false, AggregateVersion.NotCreated);
        }
        catch (PostgresException e)
        {
            if (e.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                throw new AppendOnlyStoreConcurrencyException(e, expectedStreamVersion, -1, streamName.ToString());
            }

            throw;
        }
    }

    /// <summary>
    /// Read events for specific stream.
    /// </summary>
    /// <param name="streamName">Stream name.</param>
    /// <param name="from">Position of first event.</param>
    /// <param name="to">Position of last event.</param>
    /// <returns>Events data.</returns>
    public async Task<IEventsData> ReadSpecificStream(StreamId streamName, StreamPosition from, StreamPosition to)
    {
        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync();

        await using NpgsqlBatch batch = new NpgsqlBatch(conn);

        string schemaName = SchemaName;
        NpgsqlBatchCommand getEventsCommand = _commandsBuilder.GetSelectEventsDataCommand(
            streamName,
            from,
            to,
            schemaName,
            _eventsTableName);

        batch.BatchCommands.Add(getEventsCommand);
        batch.BatchCommands.Add(_commandsBuilder.GetEventsStreamCountCommand(schemaName, _eventsTableName));
            
        await batch.PrepareAsync();

        await using NpgsqlDataReader reader = await batch.ExecuteReaderAsync();

        List<EventPackage> results = new List<EventPackage>((int)Math.Min(8192, (long)(to - from)));

        while (await reader.ReadAsync())
        {
            EventPackage package = ReadEventPackage(reader, streamName);
            results.Add(package);
        }

        await reader.NextResultAsync();
        await reader.ReadAsync();
        StreamPosition max = reader.GetInt64(0);

        await reader.CloseAsync();
        await conn.CloseAsync();

        return new EventsData(results.ToArray(), max);
    }



    /// <summary>
    /// Read event by given conditions.
    /// </summary>
    /// <param name="readOptions">Search options.</param>
    /// <returns>Events data.</returns>
    public async Task<IEventsData> ReadAllStreams(StreamReadOptions readOptions)
    {
        string commandText = BuildReadAllStreamsCommandText(readOptions);

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync();
        await using NpgsqlCommand cmd = new NpgsqlCommand(string.Format(commandText, SchemaName, _eventsTableName), conn);

        cmd.AddParameter(readOptions.To - readOptions.From);
        cmd.AddParameter(readOptions.From);

        await cmd.PrepareAsync();

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

        List<EventPackage> results = new List<EventPackage>((int)Math.Min(8192, (long)(readOptions.To - readOptions.From)));

        while (await reader.ReadAsync())
        {
            string payloadType;
            byte[] serialized;
            object? payload;

            EventPackage package = new EventPackage();
            package.EventId = reader.GetGuid(0);
            package.TenantId = reader.GetGuid(1);
            package.StreamName = new StreamId(reader.GetString(2));
            package.StreamPosition = reader.GetInt64(3);
            package.Timestamp = reader.GetFieldValue<DateTime>(4);
            switch (readOptions.ReadingVolume)
            {
                case StreamReadVolume.Data:
                    payloadType = reader.GetString(5);
                    serialized = reader.GetFieldValue<byte[]>(6);
                    payload = _serializer.Deserialize(serialized, payloadType);

                    package.Payload = payload;
                    break;

                case StreamReadVolume.Meta:
                    package.CommandId = reader.GetGuid(5);
                    package.SequenceId = reader.GetGuid(6);
                    package.PrincipalId = PrincipalId.Parse(reader.GetString(7));
                    break;

                case StreamReadVolume.MetaAndData:
                    payloadType = reader.GetString(8);
                    serialized = reader.GetFieldValue<byte[]>(9);
                    payload = _serializer.Deserialize(serialized, payloadType);

                    package.CommandId = reader.GetGuid(5);
                    package.SequenceId = reader.GetGuid(6);
                    package.PrincipalId = PrincipalId.Parse(reader.GetString(7));
                    package.Payload = payload;
                    break;
            }

            results.Add(package);
        }

        await reader.CloseAsync();
        await conn.CloseAsync();

        return new EventsData(results.ToArray(), StreamPosition.End);
    }

    /// <summary>
    /// Return streams started with given prefix.
    /// </summary>
    /// <param name="startsWithPrefix">Prefix of streams.</param>
    /// <returns>Names of streams.</returns>
    public async Task<StreamId[]> FindStreamIds(string startsWithPrefix)
    {
        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync();
        await using NpgsqlCommand cmd = new NpgsqlCommand(string.Format(SEARCH_FOR_STREAM_ID, SchemaName, _eventsTableName), conn);

        cmd.AddParameter(startsWithPrefix + "%");

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

        List<string> streams = new List<string>();
        while (await reader.ReadAsync())
        {
            streams.Add(reader[0].ToString()!);
        }

        await reader.CloseAsync();
        await conn.CloseAsync();

        return streams.Select(StreamId.Parse).ToArray();
    }
    
    /// <summary>
    /// Check whether storage exists.
    /// </summary>
    public async Task<bool> IsExist()
    {
        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync();
        await using NpgsqlCommand cmd = new NpgsqlCommand(string.Format(EXIST_COMMAND, SchemaName, _eventsTableName));

        object? result = await cmd.ExecuteScalarAsync();
        await conn.CloseAsync();

        return result is true;
    }

    /// <summary>
    /// Initialize the appender.
    /// </summary>
    public async Task Initialize()
    {
        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync();
        await using NpgsqlCommand cmd = new NpgsqlCommand(string.Format(INITIAL_COMMAND, SchemaName, _eventsTableName, _commandsTableName), conn);

        await cmd.ExecuteNonQueryAsync();
        await conn.CloseAsync();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte[] Serialize(object payload, out string type)
    {
        return _serializer.Serialize(payload, out type);
    }

    private EventPackage ReadEventPackage(NpgsqlDataReader reader, StreamId streamName)
    {
        EventPackage package = new EventPackage();
        package.StreamName = streamName;
        package.EventId = reader.GetGuid(0);
        package.StreamPosition = reader.GetInt64(1);
        package.Timestamp = reader.GetFieldValue<DateTime>(2);
        package.CommandId = reader.GetGuid(3);
        package.SequenceId = reader.GetGuid(4);
        string payloadType = reader.GetString(5);
        byte[] serialized = reader.GetFieldValue<byte[]>(6);

        object? payload = _serializer.Deserialize(serialized, payloadType);
        package.Payload = payload;

        PgStorageOptions options = _storageOptions;
        if (options.UseMultitenancy && options.StorePrincipal)
        {
            package.TenantId = reader.GetGuid(7);
            package.PrincipalId = PrincipalId.Parse(reader.GetString(8));
        }
        else
        {
            if (options.UseMultitenancy)
            {
                package.TenantId = reader.GetGuid(7);
            }

            if (options.StorePrincipal)
            {
                package.PrincipalId = PrincipalId.Parse(reader.GetString(7));
            }
        }

        return package;
    }

    private string BuildReadAllStreamsCommandText(StreamReadOptions readOptions)
    {
        StringBuilder sb = new StringBuilder(2048);
        switch (readOptions.ReadingVolume)
        {
            case StreamReadVolume.Data:
                sb.AppendLine(
                    "SELECT id, tenant_id, stream_name, stream_position, \"timestamp\", payload_type, payload");
                break;
            case StreamReadVolume.Meta:
                sb.AppendLine(
                    "SELECT id, tenant_id, stream_name, stream_position, \"timestamp\", command_id, sequence_id, principal_id, payload_type");
                break;

            default:
                sb.AppendLine(
                    "SELECT id, tenant_id, stream_name, stream_position, \"timestamp\", command_id, sequence_id, principal_id, payload_type, payload");
                break;
        }

        sb.AppendLine($"FROM \"{SchemaName}\".\"{_eventsTableName}\"");

        string like = readOptions.FilterType == AggregateStreamFilterType.Include ? "LIKE" : "NOT LIKE";
        string condition = readOptions.FilterType == AggregateStreamFilterType.Include ? "OR" : "AND";
        string where = readOptions.PrefixPattern switch
        {
            var p when p == null => string.Empty,
            var p when p.Length == 1 => $"WHERE stream_name {like} '{p[0]}%'",
            var p when p.Length > 1 => "WHERE " +
                                       string.Join($" {condition} ", p.Select(x => $"stream_name {like} '{x}%'"))
        };
        sb.AppendLine(where);
        sb.AppendLine("ORDER BY stream_position " +
                      (readOptions.ReadDirection == StreamReadDirection.Forward ? "ASC" : "DESC"));
        sb.AppendLine("LIMIT $1 OFFSET $2");

        return sb.ToString();
    }
    

}