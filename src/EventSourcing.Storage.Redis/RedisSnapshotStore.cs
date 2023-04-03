using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using CommunityToolkit.HighPerformance;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Abstractions.Types;
using EventSourcing.Core;
using EventSourcing.Core.Exceptions;
using StackExchange.Redis;

namespace EventSourcing.Storage.Redis;

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
            FromRedisValue(ref value, out SnapshotEnvelope envelope);
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

        if (_redisSnapshotCreationPolicy.Behaviour == RedisSnapshotCreationBehaviour.EveryCommit)
        {
            return SaveSnapshotInternal(streamName, snapshot);
        }

        if (snapshot.Version % _redisSnapshotCreationPolicy.CommitThreshold == 0)
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
            ToRedisValue(ref envelope, out RedisValue value);

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

    private void ToRedisValue(ref SnapshotEnvelope envelope, out RedisValue result)
    {
        int capacity = envelope.State.Length + envelope.Type.Length * 2 + 24;
        using MemoryStream ms = new MemoryStream(capacity);
        using BinaryWriter bw = new BinaryWriter(ms);
        
        bw.Write(envelope.Type);
        bw.Write(envelope.AggregateVersion);
        bw.Write(envelope.State.Length);
        bw.Write(envelope.State);
        
        byte[] rawData = ms.GetBuffer();
        Memory<byte> rawMemory = rawData.AsMemory();
        Memory<byte> data = rawMemory.Slice(0, (int)ms.Length);
        
        result = data;
    }

    private void FromRedisValue(ref RedisValue value, out SnapshotEnvelope result)
    {
        if (!value.HasValue)
        {
            result = SnapshotEnvelope.Empty;
            return;
        }

        ReadOnlyMemory<byte> memory = value;
        using Stream s = memory.AsStream();
        using BinaryReader reader = new BinaryReader(s);

        string type = reader.ReadString();
        long version = reader.ReadInt64();
        int dataLength = reader.ReadInt32();
        byte[] data = reader.ReadBytes(dataLength);

        result = new SnapshotEnvelope()
        {
            State = data,
            AggregateVersion = version,
            Type = type,
            IsEmpty = false
        };
    }
}