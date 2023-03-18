using System.Runtime.CompilerServices;
using Npgsql;
using NpgsqlTypes;

namespace EventSourcing.Storage.Postgres;

internal static class NpgSqlBatchCommandExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddParameter(this NpgsqlBatchCommand cmd, Guid value)
    {
        cmd.Parameters.Add(new NpgsqlParameter<Guid>(null, value)
        {
            NpgsqlDbType = NpgsqlDbType.Uuid
        });
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddParameter(this NpgsqlBatchCommand cmd, long value)
    {
        cmd.Parameters.Add(new NpgsqlParameter<long>(null, value)
        {
            NpgsqlDbType = NpgsqlDbType.Bigint
        });
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddParameter(this NpgsqlCommand cmd, long value)
    {
        cmd.Parameters.Add(new NpgsqlParameter<long>(null, value)
        {
            NpgsqlDbType = NpgsqlDbType.Bigint
        });
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddParameter(this NpgsqlBatchCommand cmd, DateTime value)
    {
        cmd.Parameters.Add(new NpgsqlParameter<DateTime>(null, value)
        {
            NpgsqlDbType = NpgsqlDbType.TimestampTz
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddParameter(this NpgsqlBatchCommand cmd, string value, int size = 255)
    {
        cmd.Parameters.Add(new NpgsqlParameter<string>(null, value)
        {
            NpgsqlDbType = NpgsqlDbType.Varchar,
            Size = size
        });
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddParameter(this NpgsqlCommand cmd, string value, int size = 255)
    {
        cmd.Parameters.Add(new NpgsqlParameter<string>(null, value)
        {
            NpgsqlDbType = NpgsqlDbType.Varchar,
            Size = size
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddJsonParameter(this NpgsqlBatchCommand cmd, byte[] value)
    {
        cmd.Parameters.Add(new NpgsqlParameter<byte[]>(null, value)
        {
            NpgsqlDbType = NpgsqlDbType.Jsonb
        });
    }
}