using EventSourcing.Net;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Storage.Postgres;

public sealed class PostgresOptions
{
    private readonly PgStorageOptions _storageOptions;

    internal PostgresOptions(PgStorageOptions storageOptions)
    {
        _storageOptions = storageOptions;
    }

    public PostgresOptions Configure(Action<PgStorageOptions> handler)
    {
        handler?.Invoke(_storageOptions);
        return this;
    }
}