namespace EventSourcing.Net.Storage.SQLite;

using System.Runtime.CompilerServices;
using Microsoft.Data.Sqlite;

internal static class SqliteBatchCommandExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddParameter(this SqliteCommand cmd, Guid value)
    {
        cmd.Parameters.Add(new SqliteParameter(null, value.ToString())
        {
            SqliteType = SqliteType.Text,
        });
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddParameter(this SqliteCommand cmd, long value)
    {
        cmd.Parameters.Add(new SqliteParameter(null, value)
        {
            SqliteType = SqliteType.Integer
        });
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddParameter(this SqliteCommand cmd, DateTime value)
    {
        cmd.Parameters.Add(new SqliteParameter(null, value)
        {
            SqliteType = SqliteType.Text
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddParameter(this SqliteCommand cmd, string value, int size = 255)
    {
        cmd.Parameters.Add(new SqliteParameter(null, value)
        {
            SqliteType = SqliteType.Text,
            Size = size
        });
    }
    

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddBinaryParameter(this SqliteCommand cmd, byte[] value)
    {
        cmd.Parameters.Add(new SqliteParameter(null, value)
        {
            SqliteType = SqliteType.Blob
        });
    }
}