using System.Buffers;
using System.Diagnostics;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Abstractions.Types;
using EventSourcing.Net.Engine;
using EventSourcing.Net.Engine.Exceptions;
using StackExchange.Redis;

namespace EventSourcing.Net.Storage.Redis;

/// <inheritdoc />
public sealed class RedisSnapshotStore : ISnapshotStore
{
    private readonly IRedisConnection _redisConnection;
    private readonly TenantId _tenantId;
    private readonly ISnapshotSerializerFactory _serializerFactory;
    private readonly RedisSnapshotCreationPolicy _redisSnapshotCreationPolicy;
    private readonly IRedisKeyGenerator _keyGenerator;
    private readonly ITypeMappingHandler _typeMappingHandler;

    internal RedisSnapshotStore(
        IRedisConnection redisConnection,
        ISnapshotSerializerFactory serializerFactory,
        RedisSnapshotCreationPolicy redisSnapshotCreationPolicy,
        IRedisKeyGenerator keyGenerator,
        ITypeMappingHandler typeMappingHandler,
        TenantId tenantId)
    {
        _typeMappingHandler = typeMappingHandler;
        _keyGenerator = keyGenerator;
        _redisSnapshotCreationPolicy = redisSnapshotCreationPolicy;
        _serializerFactory = serializerFactory;
        _tenantId = tenantId;
        _redisConnection = redisConnection;
    }

    /// <summary>
    /// Load snapshot.
    /// </summary>
    /// <param name="streamName">Stream name.</param>
    /// <returns>Snapshot data.</returns>
    public async Task<ISnapshot> LoadSnapshot(StreamId streamName)
    {
        byte[]? pooledArray = null;
        try
        {
            IDatabaseAsync database = _redisConnection.Connection.GetDatabase();
            RedisKey key = _keyGenerator.GetKey(_tenantId, streamName);
            RedisValue value = await database.StringGetAsync(key).ConfigureAwait(false);
            if (!value.HasValue)
            {
                return NoSnapshot(streamName);
            }

            pooledArray = ArrayPool<byte>.Shared.Rent((int)value.Length());
            RedisSnapshotEnvelopeSerializer.FromRedisValue(pooledArray, in value, out SnapshotEnvelope envelope);

            Type type = _typeMappingHandler.GetTypeById(envelope.TypeId);
            object state = _serializerFactory.GetSerializer().Deserialize(type, envelope.State);
            return new Snapshot(streamName, state, envelope.AggregateVersion);

        }
        catch (ObjectDisposedException e)
        {
            return NoSnapshot(streamName);
        }
        finally
        {
            if (pooledArray != null)
            {
                ArrayPool<byte>.Shared.Return(pooledArray);
            }
        }
    }

    /// <summary>
    /// Save snapshot to store.
    /// </summary>
    /// <param name="streamName">Stream name.</param>
    /// <param name="snapshot">Snapshot data.</param>
    public Task SaveSnapshot(StreamId streamName, ISnapshot? snapshot)
    {
        if (snapshot == null)
        {
            Thrown.ArgumentNullException(nameof(snapshot));
        }
        if (!snapshot.HasSnapshot)
        {
            return Task.CompletedTask;
        }

        if (_redisSnapshotCreationPolicy.Behaviour == RedisSnapshotCreationBehaviour.EveryCommit &&
            snapshot.Version > _redisSnapshotCreationPolicy.MinAggregateVersion)
        {
            return SaveSnapshotInternal(streamName, snapshot);
        }

        if (snapshot.Version % _redisSnapshotCreationPolicy.CommitThreshold == 0 &&
            snapshot.Version > _redisSnapshotCreationPolicy.MinAggregateVersion)
        {
            return SaveSnapshotInternal(streamName, snapshot);
        }

        return Task.CompletedTask;
    }

    private async Task SaveSnapshotInternal(StreamId streamName, ISnapshot snapshot)
    {
        if (snapshot.State == null)
        {
            Trace.WriteLine($"Snapshot.State for {streamName.ToString()} is null");
            return;
        }

        byte[]? pooledArray = null;
        try
        {
            IDatabaseAsync database = _redisConnection.Connection.GetDatabase();
            TypeMappingId typeId = _typeMappingHandler.GetIdByType(snapshot.State.GetType());
            byte[] data = _serializerFactory.GetSerializer().Serialize(snapshot.State);
            SnapshotEnvelope envelope = new SnapshotEnvelope()
            {
                State = data,
                AggregateVersion = snapshot.Version,
                TypeId = typeId
            };
            pooledArray = ArrayPool<byte>.Shared.Rent(RedisSnapshotEnvelopeSerializer.GetSize(data.Length));
            RedisSnapshotEnvelopeSerializer.ToRedisValue(pooledArray, in envelope, out RedisValue value);

            RedisKey key = _keyGenerator.GetKey(_tenantId, streamName);
            TimeSpan? expire = _redisSnapshotCreationPolicy.ExpireAfter != TimeSpan.Zero
                ? _redisSnapshotCreationPolicy.ExpireAfter
                : null;
            await database
                .StringSetAsync(key, value, expire, When.Always, CommandFlags.FireAndForget)
                .ConfigureAwait(false);
        }
        catch (ObjectDisposedException e)
        {
            // connection might be disposed during reconnect
        }
        finally
        {
            if (pooledArray != null)
            {
                ArrayPool<byte>.Shared.Return(pooledArray);
            }
        }
    }
    
    private ISnapshot NoSnapshot(StreamId streamName) => new Snapshot()
    {
        State = null,
        StreamName = streamName,
        Version = AggregateVersion.NotCreated
    };
}