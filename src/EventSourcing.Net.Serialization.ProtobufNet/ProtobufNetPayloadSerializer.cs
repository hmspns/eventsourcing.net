using System.Buffers;
using EventSourcing.Net.Abstractions.Contracts;
using ProtoBuf;
using ProtoBuf.Meta;

namespace EventSourcing.Net.Serialization.ProtobufNet;

using Engine.Pooled;

public sealed class ProtobufNetPayloadSerializer : IPayloadSerializer
{
    public byte[] Serialize(object obj)
    {
        using PooledMemoryStream ms = new PooledMemoryStream();
        RuntimeTypeModel.Default.Serialize(ms, obj);
        return ms.ToArray();
    }

    public object Deserialize(Type type, Memory<byte> data)
    {
        object? result = RuntimeTypeModel.Default.Deserialize(type, data);
        return result;
    }
}