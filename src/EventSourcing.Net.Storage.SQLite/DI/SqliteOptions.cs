namespace EventSourcing.Net.Storage.SQLite.DI;

public sealed class SqliteOptions
{
    private readonly SqliteStorageOptions _storageOptions;

    internal SqliteOptions(SqliteStorageOptions storageOptions)
    {
        _storageOptions = storageOptions;
    }

    public SqliteOptions Configure(Action<SqliteStorageOptions> handler)
    {
        handler?.Invoke(_storageOptions);
        return this;
    }
}