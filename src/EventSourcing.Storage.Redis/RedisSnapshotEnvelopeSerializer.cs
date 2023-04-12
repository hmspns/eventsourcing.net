using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Core.Exceptions;
using StackExchange.Redis;

namespace EventSourcing.Storage.Redis;

internal static class RedisSnapshotEnvelopeSerializer
{
    private const int ID_LENGTH = 16;
    private const int VERSION_LENGTH = 8;

    internal static void ToRedisValueOld(ref SnapshotEnvelope envelope, out RedisValue result)
    {
        int capacity = envelope.State.Length + ID_LENGTH + 24;
        using MemoryStream ms = new MemoryStream(capacity);
        using BinaryWriter bw = new BinaryWriter(ms);

        bw.Write(envelope.TypeId.Id.ToByteArray());
        bw.Write(envelope.AggregateVersion);
        //bw.Write(envelope.State.Length);
        bw.Write(envelope.State);
        
        byte[] rawData = ms.GetBuffer();
        Memory<byte> rawMemory = rawData.AsMemory();
        Memory<byte> data = rawMemory.Slice(0, (int)ms.Length);
        
        result = data;
    }

    internal static void ToRedisValue(byte[] pooledArray, ref SnapshotEnvelope envelope, out RedisValue result)
    {
        int size = GetSize(envelope.State);
        Memory<byte> resultMemory = pooledArray.AsMemory().Slice(0, size);
        Span<byte> id = resultMemory.Slice(0, ID_LENGTH).Span;
        if (!envelope.TypeId.Id.TryWriteBytes(id))
        {
            Thrown.InvalidOperationException("Couldn't write id");
        }

        Span<byte> version = resultMemory.Slice(ID_LENGTH, VERSION_LENGTH).Span;
        BinaryPrimitives.WriteInt64LittleEndian(version, envelope.AggregateVersion);
        envelope.State.CopyTo(resultMemory.Slice(ID_LENGTH + VERSION_LENGTH));
        result = resultMemory;
    }

    internal static void FromRedisValue(byte[] pooledArray, ref RedisValue value, out SnapshotEnvelope result)
    {
        ReadOnlyMemory<byte> memory = value;
        Guid id = new Guid(memory.Slice(0, ID_LENGTH).Span);
        long version = BinaryPrimitives.ReadInt64LittleEndian(memory.Slice(ID_LENGTH, VERSION_LENGTH).Span);

        Memory<byte> pooledMemory = new Memory<byte>(pooledArray);
        memory.Slice(ID_LENGTH + VERSION_LENGTH).CopyTo(pooledMemory);
        //byte[] data = memory.Slice(ID_LENGTH + VERSION_LENGTH).ToArray();
        TypeMappingId type = new TypeMappingId(id);

        result = new SnapshotEnvelope()
        {
            //State = data,
            AggregateVersion = version,
            TypeId = type,
            MemoryState = pooledMemory.Slice(0, memory.Length - ID_LENGTH - VERSION_LENGTH)
        };
    }

    internal static void FromRedisValueOld(ref RedisValue value, out SnapshotEnvelope result)
    {
        ReadOnlyMemory<byte> memory = value;
        using Stream s = memory.AsStream();
        using BinaryReader reader = new BinaryReader(s);

        byte[] id = reader.ReadBytes(ID_LENGTH);
        long version = reader.ReadInt64();
        int dataLength = memory.Length - ID_LENGTH - VERSION_LENGTH;
        byte[] data = reader.ReadBytes(dataLength);
        
        TypeMappingId type = new TypeMappingId(new Guid(id));

        result = new SnapshotEnvelope()
        {
            State = data,
            AggregateVersion = version,
            TypeId = type,
        };
    }
    
    internal static int GetSize(byte[] state)
    {
        int size = ID_LENGTH + //TypeId
                   VERSION_LENGTH + //AggregateVersion
                   state.Length;
        return size;
    }
}