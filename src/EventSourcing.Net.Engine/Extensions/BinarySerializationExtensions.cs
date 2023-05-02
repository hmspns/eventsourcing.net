using System;
using System.IO;
using System.Runtime.CompilerServices;
using EventSourcing.Net.Engine.LowLevel;

namespace EventSourcing.Net.Engine.Extensions;

internal static class BinarySerializationExtensions
{
    internal static void Write(this BinaryWriter writer, CommandEntry entry)
    {
        writer.Write(entry.CommandId);
        writer.Write(entry.ParentCommandId);
        writer.Write(entry.CommandSequenceId);
        writer.Write(entry.TenantId);
        writer.Write(entry.Timestamp);
        writer.Write(entry.AggregateId);
        writer.Write(entry.PrincipalId);
        writer.Write(entry.CommandSource);
        writer.Write(entry.PayloadType);
        writer.WritePayload(entry.Payload);
    }

    internal static void Write(this BinaryWriter writer, EventEntry entry)
    {
        writer.Write(entry.Id);
        writer.Write(entry.TenantId);
        writer.Write(entry.StreamName);
        writer.Write(entry.StreamPosition);
        writer.Write(entry.GlobalPosition);
        writer.Write(entry.Timestamp);
        writer.Write(entry.CommandId);
        writer.Write(entry.SequenceId);
        writer.Write(entry.PrincipalId);
        writer.Write(entry.PayloadType);
        writer.WritePayload(entry.Payload);
    }
    
    internal static CommandEntry ReadCommandEntry(this BinaryReader reader)
    {
        CommandEntry entry = new CommandEntry();
        entry.CommandId = reader.ReadGuid();
        entry.ParentCommandId = reader.ReadGuid();
        entry.CommandSequenceId = reader.ReadGuid();
        entry.TenantId = reader.ReadGuid();
        entry.Timestamp = reader.ReadTimestamp();
        entry.AggregateId = reader.ReadString();
        entry.PrincipalId = reader.ReadString();
        entry.CommandSource = reader.ReadString();
        entry.PayloadType = reader.ReadString();
        entry.Payload = reader.ReadPayload();

        return entry;
    }

    internal static EventEntry ReadEventEntry(this BinaryReader reader)
    {
        EventEntry entry = new EventEntry();
        entry.Id = reader.ReadGuid();
        entry.TenantId = reader.ReadGuid();
        entry.StreamName = reader.ReadString();
        entry.StreamPosition = reader.ReadInt64();
        entry.GlobalPosition = reader.ReadInt64();
        entry.Timestamp = reader.ReadTimestamp();
        entry.CommandId = reader.ReadGuid();
        entry.SequenceId = reader.ReadGuid();
        entry.PrincipalId = reader.ReadString();
        entry.PayloadType = reader.ReadString();
        entry.Payload = reader.ReadPayload();

        return entry;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Guid ReadGuid(this BinaryReader reader)
    {
        return new Guid(reader.ReadBytes(16));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Write(this BinaryWriter writer, Guid id)
    {
        writer.Write(id.ToByteArray());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Write(this BinaryWriter writer, DateTime timestamp)
    {
        writer.Write(timestamp.ToBinary());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static DateTime ReadTimestamp(this BinaryReader reader)
    {
        return DateTime.FromBinary(reader.ReadInt64());
    }
    
    private static void WritePayload(this BinaryWriter writer, byte[] payload)
    {
        writer.Write(payload.Length);
        writer.Write(payload);
    }
    
    private static byte[] ReadPayload(this BinaryReader reader)
    {
        int payloadLength = reader.ReadInt32();
        return reader.ReadBytes(payloadLength);
    }
}