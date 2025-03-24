namespace EventSourcing.Net.Storage.SQLite;

using System.Data;
using System.Runtime.CompilerServices;
using Commands;
using Abstractions.Contracts;
using Abstractions.Identities;
using Abstractions.Types;
using Engine;
using Engine.Exceptions;
using Engine.Pooled.Collections;
using Microsoft.Data.Sqlite;

public sealed class SqliteSqlAppender : IAppendOnly
{
    private readonly IPayloadSerializer _serializer;
    private readonly SqliteDataSource _dataSource;
    private readonly ISqliteCommandsBuilder _commandsBuilder;
    private readonly SqliteStorageOptions _storageOptions;
    private readonly TenantId _tenantId;
    
    private readonly string _eventsTableName;
    private readonly string _commandsTableName;
    private readonly ITypeMappingHandler _typeMappingHandler;

    private string SchemaName
    {
        get
        {
            return _storageOptions.UseMultitenancy ?
                _storageOptions.MultitenancySchemaName(_tenantId) :
                _storageOptions.NonMultitenancySchemaName;
        }
    }

    internal SqliteSqlAppender(
        IPayloadSerializer serializer,
        SqliteDataSource dataSource,
        ISqliteCommandsBuilder commandsBuilder,
        SqliteStorageOptions storageOptions,
        ITypeMappingHandler typeMappingHandler,
        TenantId tenantId)
    {
        _dataSource = dataSource;
        _typeMappingHandler = typeMappingHandler;
        _tenantId = tenantId;
        _commandsBuilder = commandsBuilder;
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
    public Task<IAppendEventsResult> Append<TId>(
        StreamId streamName,
        IAppendDataPackage<TId> data,
        AggregateVersion expectedStreamVersion,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using SqliteConnection conn = _dataSource.OpenConnection();
            using SqliteTransaction transaction = conn.BeginTransaction(IsolationLevel.RepeatableRead);
            using SqliteCommand selectVersionCommand = _commandsBuilder.GetStreamVersionCommand(streamName, SchemaName, _eventsTableName);
            
            selectVersionCommand.Connection = conn;
            selectVersionCommand.Prepare();

            long version = (long)selectVersionCommand.ExecuteScalar()!;

            if (version != expectedStreamVersion)
            {
                throw new AppendOnlyStoreConcurrencyException(expectedStreamVersion, version, streamName.ToString());
            }
            
            long position = expectedStreamVersion;
            
            TypeMappingId aggregateIdType = _typeMappingHandler.GetIdByType(data.CommandPackage.AggregateId.GetType());

            using PooledList<SqliteCommand> commands = new PooledList<SqliteCommand>(GetCommandsCount());
            
            foreach (IAppendEventPackage appendPackage in data.EventPackages)
            {
                position += 1;
                byte[] eventPayload = Serialize(appendPackage.Payload, out TypeMappingId eventPayloadType);
                InsertEventCommandArguments<TId> commandArguments = new InsertEventCommandArguments<TId>()
                {
                    Data = data,
                    AppendPackage = appendPackage,
                    PayloadType = eventPayloadType,
                    SchemaName = SchemaName,
                    Payload = eventPayload,
                    EventsTableName = _eventsTableName,
                    Position = position,
                    AggregateIdType = aggregateIdType
                };
                SqliteCommand cmd = _commandsBuilder.GetInsertEventCommand(ref commandArguments);
                commands.Add(cmd);
            }

            if (_storageOptions.StoreCommands)
            {
                byte[] commandPayload = Serialize(data.CommandPackage.Payload, out TypeMappingId commandPayloadType);
                SqliteCommand createCommandCmd = _commandsBuilder.GetInsertCommandCommand(
                    data,
                    commandPayload,
                    commandPayloadType,
                    SchemaName,
                    _commandsTableName);
                commands.Add(createCommandCmd);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<IAppendEventsResult>(cancellationToken);
            }

            foreach (SqliteCommand command in commands)
            {
                command.ExecuteNonQuery();
            }
            

            if (!cancellationToken.IsCancellationRequested)
            {
                transaction.Commit();

                return Result(new AppendEventsResult(true, position));
            }

            return Result(new AppendEventsResult(false, AggregateVersion.NotCreated));
        }
        catch (SqliteException e)
        {
            // https://www.sqlite.org/rescode.html
            if (e.SqliteExtendedErrorCode == 2067 // UniqueViolation 
               || e.SqliteExtendedErrorCode == 517 // transaction failure
               || e.ErrorCode == 5 // busy
               )
            {
                throw new AppendOnlyStoreConcurrencyException(e, expectedStreamVersion, -1, streamName.ToString());
            }

            throw;
        }

        int GetCommandsCount()
        {
            int count = 0;
            if (data.EventPackages.TryGetNonEnumeratedCount(out int eventCount))
            {
                count += eventCount;
            }

            if (_storageOptions.StoreCommands)
            {
                count += 1;
            }
            
            return count > 0 ? count : 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Task<IAppendEventsResult> Result(AppendEventsResult result)
        {
            return Task.FromResult((IAppendEventsResult)result);
        }
    }

    /// <summary>
    /// Read events for specific stream.
    /// </summary>
    /// <param name="streamName">Stream name.</param>
    /// <param name="from">Position of first event.</param>
    /// <param name="to">Position of last event.</param>
    /// <returns>Events data.</returns>
    public Task<IEventsData> ReadSpecificStream(StreamId streamName, StreamPosition from, StreamPosition to)
    {
        using SqliteConnection conn = _dataSource.OpenConnection();
        
        string schemaName = SchemaName;
        SqliteCommand getEventsCommand = _commandsBuilder.GetSelectEventsDataCommand(
            streamName,
            from,
            to,
            schemaName,
            _eventsTableName);
        
        
        using SqliteDataReader reader = getEventsCommand.ExecuteReader();

        PooledList<EventPackage> results = new PooledList<EventPackage>((int)Math.Min(32, (long)(to - from)));

        while (reader.Read())
        {
            ReadEventPackage(reader, streamName, out EventPackage package);
            results.Add(package);
        }
        
        reader.Close();
        
        SqliteCommand eventsStreamCountCommand = _commandsBuilder.GetEventsStreamCountCommand(schemaName, _eventsTableName);

        StreamPosition max = (long)eventsStreamCountCommand.ExecuteScalar();

        return Task.FromResult((IEventsData)new EventsData(results, max));
    }
    
    /// <summary>
    /// Read event by given conditions.
    /// </summary>
    /// <param name="readOptions">Search options.</param>
    /// <returns>Events data.</returns>
    public Task<IExtendedEventsData> ReadAllStreams(StreamReadOptions readOptions)
    {
        using SqliteConnection conn = _dataSource.OpenConnection();
        using SqliteCommand cmd = _commandsBuilder.GetReadAllStreamsCommand(readOptions, SchemaName, _eventsTableName);
        cmd.Connection = conn;
        
        using SqliteDataReader reader = cmd.ExecuteReader();

        PooledList<ExtendedEventPackage> results = new PooledList<ExtendedEventPackage>((int)Math.Min(8192, (long)(readOptions.To - readOptions.From)));

        while (reader.Read())
        {
            ExtendedEventPackage package = ReadEventPackage(reader, readOptions);

            results.Add(package);
        }

        reader.Close();
        conn.Close();

        return Task.FromResult((IExtendedEventsData)new ExtendedEventsData(results, StreamPosition.End));
    }

    /// <summary>
    /// Return streams started with given prefix.
    /// </summary>
    /// <param name="startsWithPrefix">Prefix of streams.</param>
    /// <returns>Names of streams.</returns>
    public Task<StreamId[]> FindStreamIds(string startsWithPrefix)
    {
        using SqliteConnection conn = _dataSource.OpenConnection();
        using SqliteCommand cmd = _commandsBuilder.GetFindStreamIdsByPatternCommand(startsWithPrefix, SchemaName, _eventsTableName);
        cmd.Connection = conn;

        using SqliteDataReader reader = cmd.ExecuteReader();

        List<string> streams = new List<string>();
        while (reader.Read())
        {
            streams.Add(reader[0].ToString()!);
        }

        reader.CloseAsync();
        conn.CloseAsync();

        return Task.FromResult(streams.Select(StreamId.Parse).ToArray());
    }
    
    /// <summary>
    /// Check whether storage exists.
    /// </summary>
    public Task<bool> IsExist()
    {
        using SqliteConnection conn = _dataSource.OpenConnection();

        using SqliteCommand cmd = _commandsBuilder.GetCheckStorageExistsCommand(SchemaName, _eventsTableName);
        cmd.Connection = conn;

        object? result = cmd.ExecuteScalar();
        conn.Close();

        return Task.FromResult(result is true);
    }

    /// <summary>
    /// Initialize the appender.
    /// </summary>
    public Task Initialize()
    {
        using SqliteConnection conn = _dataSource.OpenConnection();

        using SqliteCommand cmd = _commandsBuilder.GetCreateStorageCommand(SchemaName, _eventsTableName, _commandsTableName);
        cmd.Connection = conn;
        
        cmd.ExecuteNonQuery();
        conn.Close();

        return Task.CompletedTask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte[] Serialize(object payload, out TypeMappingId id)
    {
        Type type = payload.GetType();
        id = _typeMappingHandler.GetIdByType(type);
        return _serializer.Serialize(payload);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private object Deserialize(TypeMappingId id, Memory<byte> data)
    {
        Type type = _typeMappingHandler.GetTypeById(id);
        return _serializer.Deserialize(type, data);
    }

    private void ReadEventPackage(SqliteDataReader reader, StreamId streamName, out EventPackage package)
    {
        Unsafe.SkipInit(out package);
        package.StreamName = streamName;
        package.EventId = reader.GetGuid(0);
        package.StreamPosition = reader.GetInt64(1);
        package.Timestamp = reader.GetFieldValue<DateTime>(2);
        package.CommandId = reader.GetGuid(3);
        package.SequenceId = reader.GetGuid(4);

        Guid payloadType = reader.GetGuid(5);
        byte[] serialized = reader.GetFieldValue<byte[]>(6);

        object payload = Deserialize(payloadType, serialized);
        package.Payload = payload;

        int principalIdPosition = 7;
        SqliteStorageOptions options = _storageOptions;
        
        if (options.StoreTenantId)
        {
            package.TenantId = reader.GetGuid(7);
            principalIdPosition = 8;
        }
        else
        {
            package.TenantId = _tenantId;
        }
        
        package.PrincipalId = options.StorePrincipal ? PrincipalId.Parse(reader.GetString(principalIdPosition)) : PrincipalId.Empty;
    }

    private ExtendedEventPackage ReadEventPackage(SqliteDataReader reader, StreamReadOptions readOptions)
    {
        Guid payloadType;
        byte[] serialized;
        object? payload;

        ExtendedEventPackage package = new ExtendedEventPackage();
        package.EventId = reader.GetGuid(SqliteCommandTextProvider.ID);
        package.StreamName = new StreamId(reader.GetString(SqliteCommandTextProvider.STREAM_NAME));
        package.StreamPosition = reader.GetInt64(SqliteCommandTextProvider.STREAM_POSITION);
        package.Timestamp = reader.GetFieldValue<DateTime>(SqliteCommandTextProvider.TIMESTAMP);
        package.AggregateTypeId = reader.GetGuid(SqliteCommandTextProvider.AGGREGATE_ID_TYPE);
        switch (readOptions.ReadingVolume)
        {
            case StreamReadVolume.Data:
                payloadType = reader.GetGuid(SqliteCommandTextProvider.PAYLOAD_TYPE);
                serialized = reader.GetFieldValue<byte[]>(SqliteCommandTextProvider.PAYLOAD);
                payload = Deserialize(payloadType, serialized);

                package.Payload = payload;
                break;

            case StreamReadVolume.Meta:
                package.CommandId = reader.GetGuid(SqliteCommandTextProvider.COMMAND_ID);
                package.SequenceId = reader.GetGuid(SqliteCommandTextProvider.SEQUENCE_ID);
                if (_storageOptions.StorePrincipal)
                {
                    package.PrincipalId = PrincipalId.Parse(reader.GetString(SqliteCommandTextProvider.PRINCIPAL_ID));
                }

                break;

            case StreamReadVolume.MetaAndData:
                payloadType = reader.GetGuid(SqliteCommandTextProvider.PAYLOAD_TYPE);
                serialized = reader.GetFieldValue<byte[]>(SqliteCommandTextProvider.PAYLOAD);
                payload = Deserialize(payloadType, serialized);

                package.CommandId = reader.GetGuid(SqliteCommandTextProvider.COMMAND_ID);
                package.SequenceId = reader.GetGuid(SqliteCommandTextProvider.SEQUENCE_ID);
                package.Payload = payload;
                if (_storageOptions.StorePrincipal)
                {
                    package.PrincipalId = PrincipalId.Parse(reader.GetString(SqliteCommandTextProvider.PRINCIPAL_ID));
                }
                break;
        }

        if (_storageOptions.StoreTenantId)
        {
            package.TenantId = reader.GetGuid(SqliteCommandTextProvider.TENANT_ID);
        }
        else
        {
            package.TenantId = _tenantId;
        }

        return package;
    }
}