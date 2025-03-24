namespace EventSourcing.Net.Storage.SQLite;

using Microsoft.Data.Sqlite;

public sealed class SqliteDataSource
{
    private readonly string _connectionString;

    public SqliteDataSource(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public SqliteConnection OpenConnection()
    {
        SqliteConnection connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }
}