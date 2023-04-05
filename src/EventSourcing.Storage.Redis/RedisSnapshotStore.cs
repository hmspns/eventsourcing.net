using System.Diagnostics;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Abstractions.Types;
using EventSourcing.Core;
using EventSourcing.Core.Exceptions;
using StackExchange.Redis;

namespace EventSourcing.Storage.Redis;

/// <inheritdoc />
public sealed class RedisSnapshotStore : ISnapshotStore
{
    private readonly IRedisConnection _redisConnection;
    private readonly TenantId _tenantId;
    private readonly ISnapshotsSerializerFactory _serializerFactory;
    private readonly RedisSnapshotCreationPolicy _redisSnapshotCreationPolicy;
    private readonly IRedisKeyGenerator _keyGenerator;

    internal RedisSnapshotStore(
        IRedisConnection redisConnection,
        ISnapshotsSerializerFactory serializerFactory,
        RedisSnapshotCreationPolicy redisSnapshotCreationPolicy,
        IRedisKeyGenerator keyGenerator,
        TenantId tenantId)
    {
        _keyGenerator = keyGenerator;
        _redisSnapshotCreationPolicy = redisSnapshotCreationPolicy;
        _serializerFactory = serializerFactory;
        _tenantId = tenantId;
        _redisConnection = redisConnection;
    }

    public async Task<ISnapshot> LoadSnapshot(StreamId streamName)
    {
        try
        {
            IDatabaseAsync database = _redisConnection.Connection.GetDatabase();
            RedisKey key = _keyGenerator.GetKey(_tenantId, streamName);
            RedisValue value = await database.StringGetAsync(key);
            RedisSnapshotEnvelopeSerializer.FromRedisValue(ref value, out SnapshotEnvelope envelope);
            if (envelope.IsEmpty)
            {
                return NoSnapshot(streamName);
            }

            object? state = _serializerFactory.Get().Deserialize(envelope.State, envelope.Type);
            return new Snapshot(streamName, state, envelope.AggregateVersion);

        }
        catch (ObjectDisposedException e)
        {
            return NoSnapshot(streamName);
        }
    }

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

        try
        {
            IDatabaseAsync database = _redisConnection.Connection.GetDatabase();
            byte[] data = _serializerFactory.Get().Serialize(snapshot.State, out string type);
            SnapshotEnvelope envelope = new SnapshotEnvelope()
            {
                State = data,
                AggregateVersion = snapshot.Version,
                Type = type
            };
            RedisSnapshotEnvelopeSerializer.ToRedisValue(ref envelope, out RedisValue value);

            RedisKey key = _keyGenerator.GetKey(_tenantId, streamName);
            TimeSpan? expire = _redisSnapshotCreationPolicy.ExpireAfter != TimeSpan.Zero
                ? _redisSnapshotCreationPolicy.ExpireAfter
                : null;
            await database.StringSetAsync(key, value, expire, When.Always, CommandFlags.FireAndForget);
        }
        catch (ObjectDisposedException e)
        {
            // connection might be disposed during reconnect
        }
    }
    
    private ISnapshot NoSnapshot(StreamId streamName) => new Snapshot()
    {
        State = null,
        StreamName = streamName,
        Version = AggregateVersion.NotCreated
    };
}