using EventSourcing.Net;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Storage.Postgres;

public class PostgresOptions
{
    private readonly PgStorageOptions _storageOptions = new PgStorageOptions();

    public PostgresOptions(IServiceCollection services)
    {
        services.AddSingleton<IPgCommandTextProvider, PgCommandTextProvider>();
        services.AddSingleton<IPgCommandsBuilder, PgCommandsBuilder>();
        services.AddSingleton<PgStorageOptions>(_storageOptions);
    }

    public PostgresOptions Configure(Action<PgStorageOptions> handler)
    {
        handler?.Invoke(_storageOptions);
        return this;
    }
}