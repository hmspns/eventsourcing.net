using System.Buffers.Binary;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Core.Exceptions;
using StackExchange.Redis;

namespace EventSourcing.Storage.Redis;

internal static class RedisSnapshotEnvelopeSerializer
{
    private const int ID_LENGTH = 16;
    private const int VERSION_LENGTH = 8;

    internal static void ToRedisValue(byte[] pooledArray, ref SnapshotEnvelope envelope, out RedisValue result)
    {
        int size = GetSize(envelope.State.Length);
        Memory<byte> resultMemory = new Memory<byte>(pooledArray).Slice(0, size);
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
        ReadOnlyMemory<byte> redisValueMemory = value;
        Guid id = new Guid(redisValueMemory.Slice(0, ID_LENGTH).Span);
        long version = BinaryPrimitives.ReadInt64LittleEndian(redisValueMemory.Slice(ID_LENGTH, VERSION_LENGTH).Span);

        Memory<byte> pooledMemory = new Memory<byte>(pooledArray);
        redisValueMemory.Slice(ID_LENGTH + VERSION_LENGTH).CopyTo(pooledMemory);
        TypeMappingId type = new TypeMappingId(id);

        result = new SnapshotEnvelope()
        {
            AggregateVersion = version,
            TypeId = type,
            State = pooledMemory.Slice(0, redisValueMemory.Length - ID_LENGTH - VERSION_LENGTH)
        };
    }

    internal static int GetSize(int stateLength)
    {
        int size = ID_LENGTH + //TypeId
                   VERSION_LENGTH + //AggregateVersion
                   stateLength;
        return size;
    }
}