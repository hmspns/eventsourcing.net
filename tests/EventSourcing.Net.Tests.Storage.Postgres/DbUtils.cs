namespace EventSourcing.Net.Tests.Storage.Postgres;

using Npgsql;

public sealed class DbUtils
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _userName;
    private readonly string _password;

    public DbUtils(string host = "127.0.0.1", int port = 5432, string userName = "postgres", string password = "P@ssw0rd")
    {
        _password = password;
        _userName = userName;
        _port = port;
        _host = host;
    }

    private NpgsqlConnectionStringBuilder GetBuilder()
    {
        NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder
        {
            Host = _host,
            Port = _port,
            Username = _userName,
            Password = _password,
            IncludeErrorDetail = true
        };
        return builder;
    }

    public string GetConnectionString(string dbName)
    {
        NpgsqlConnectionStringBuilder builder = GetBuilder();
        builder.Database = dbName;
        return builder.ToString();
    }

    public async Task CreateDb(string dbName)
    {
        try
        {
            NpgsqlConnectionStringBuilder builder = GetBuilder();
            await using NpgsqlDataSource source = NpgsqlDataSource.Create(builder);
            await using NpgsqlCommand cmd = source.CreateCommand($"CREATE DATABASE {dbName}");
            await cmd.ExecuteNonQueryAsync();
        }
        catch (PostgresException e)
        {
            if (e.ErrorCode != -2147467259) // db exists
            {
                throw;
            }
        }
    }

    public async Task DropDb(string dbName)
    {
        NpgsqlConnectionStringBuilder builder = GetBuilder();
        await using NpgsqlDataSource source = NpgsqlDataSource.Create(builder);
        await using NpgsqlCommand cmd = source.CreateCommand(@$"
SELECT pg_terminate_backend(pg_stat_activity.pid)
FROM pg_stat_activity
WHERE pg_stat_activity.datname = '{dbName}' -- ← change this to your DB
  AND pid <> pg_backend_pid();");
        await using NpgsqlCommand dropCmd = source.CreateCommand($"DROP DATABASE {dbName}");

        await cmd.ExecuteNonQueryAsync();
        await dropCmd.ExecuteNonQueryAsync();
    }
}