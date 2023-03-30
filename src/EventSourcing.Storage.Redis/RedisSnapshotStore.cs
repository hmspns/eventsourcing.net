﻿using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using CommunityToolkit.HighPerformance;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Abstractions.Types;
using EventSourcing.Core;
using StackExchange.Redis;

namespace EventSourcing.Storage.Redis;

public sealed class RedisSnapshotStore : ISnapshotStore
{
    private readonly IRedisConnection _redisConnection;
    private readonly TenantId _tenantId;
    private readonly IPayloadSerializer _payloadSerializer;

    internal RedisSnapshotStore(IRedisConnection redisConnection, IPayloadSerializer payloadSerializer,
        TenantId tenantId)
    {
        _payloadSerializer = payloadSerializer;
        _tenantId = tenantId;
        _redisConnection = redisConnection;
    }

    public async Task<ISnapshot> LoadSnapshot(StreamId streamName)
    {
        try
        {
            IDatabaseAsync database = _redisConnection.Connection.GetDatabase();
            RedisValue value = await database.HashGetAsync(_tenantId.ToString(), streamName.ToString());
            SnapshotEnvelope envelope = FromRedisValue(ref value);
            if (envelope.IsEmpty)
            {
                return NoSnapshot(streamName);
            }

            object state = _payloadSerializer.Deserialize(envelope.State, envelope.Type);
            return new Snapshot(streamName, state, envelope.AggregateVersion);

        }
        catch (ObjectDisposedException e)
        {
            return NoSnapshot(streamName);
        }
    }

    public async Task SaveSnapshot(StreamId streamName, ISnapshot snapshot)
    {
        if (snapshot == null)
        {
            throw new ArgumentNullException(nameof(snapshot));
        }

        if (snapshot.State == null)
        {
            Trace.WriteLine($"Snapshot.State for {streamName.ToString()} is null");
            return;
        }

        try
        {
            IDatabaseAsync database = _redisConnection.Connection.GetDatabase();
            byte[] data = _payloadSerializer.Serialize(snapshot.State, out string type);
            SnapshotEnvelope envelope = new SnapshotEnvelope()
            {
                State = data,
                AggregateVersion = snapshot.Version,
                Type = type
            };
            RedisValue value = ToRedisValue(ref envelope);

            await database.HashSetAsync(_tenantId.ToString(), streamName.ToString(), value);
        }
        catch (ObjectDisposedException e)
        {
            // connection might be disposed during reconnect
        }
    }
    
    private ISnapshot NoSnapshot(StreamId streamName) => new Snapshot()
    {
        State = null,
        HasSnapshot = false,
        StreamName = streamName,
        Version = AggregateVersion.NotCreated
    };

    private RedisValue ToRedisValue(ref SnapshotEnvelope envelope)
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
        
        RedisValue result = data;
        return result;
    }

    private SnapshotEnvelope FromRedisValue(ref RedisValue value)
    {
        if (!value.HasValue)
        {
            return SnapshotEnvelope.Empty;
        }

        ReadOnlyMemory<byte> memory = value;
        using Stream s = memory.AsStream();
        using BinaryReader reader = new BinaryReader(s);

        string type = reader.ReadString();
        long version = reader.ReadInt64();
        int dataLength = reader.ReadInt32();
        byte[] data = reader.ReadBytes(dataLength);

        return new SnapshotEnvelope()
        {
            State = data,
            AggregateVersion = version,
            Type = type,
            IsEmpty = false
        };
    }
}