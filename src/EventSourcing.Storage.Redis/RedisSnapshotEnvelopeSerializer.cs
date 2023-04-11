using CommunityToolkit.HighPerformance;
using EventSourcing.Abstractions.Identities;
using StackExchange.Redis;

namespace EventSourcing.Storage.Redis;

internal static class RedisSnapshotEnvelopeSerializer
{
    internal static void ToRedisValue(ref SnapshotEnvelope envelope, out RedisValue result)
    {
        int capacity = envelope.State.Length + 16 + 24;
        using MemoryStream ms = new MemoryStream(capacity);
        using BinaryWriter bw = new BinaryWriter(ms);
        
        bw.Write(envelope.TypeId.Id.ToByteArray());
        bw.Write(envelope.AggregateVersion);
        bw.Write(envelope.State.Length);
        bw.Write(envelope.State);
        
        byte[] rawData = ms.GetBuffer();
        Memory<byte> rawMemory = rawData.AsMemory();
        Memory<byte> data = rawMemory.Slice(0, (int)ms.Length);
        
        result = data;
    }

    internal static void FromRedisValue(ref RedisValue value, out SnapshotEnvelope result)
    {
        if (!value.HasValue)
        {
            result = SnapshotEnvelope.Empty;
            return;
        }

        ReadOnlyMemory<byte> memory = value;
        using Stream s = memory.AsStream();
        using BinaryReader reader = new BinaryReader(s);

        byte[] source = reader.ReadBytes(16);
        long version = reader.ReadInt64();
        int dataLength = reader.ReadInt32();
        byte[] data = reader.ReadBytes(dataLength);
        
        TypeMappingId type = new TypeMappingId(new Guid(source));

        result = new SnapshotEnvelope()
        {
            State = data,
            AggregateVersion = version,
            TypeId = type,
            IsEmpty = false
        };
    }
}