using EventSourcing.Net;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Storage.Postgres;

public class PostgresOptions
{
    private readonly IServiceCollection _services;

    public PostgresOptions(IServiceCollection services)
    {
        _services = services;

        _services.AddSingleton<IPgCommandTextProvider, PgCommandTextProvider>();
        _services.AddSingleton<IPgCommandsBuilder, PgCommandsBuilder>();
        _services.AddSingleton<PgStorageOptions>();
    }

    public PostgresOptions UseCommandTextProvider(IPgCommandTextProvider provider)
    {
        _services.Replace<IPgCommandTextProvider>(x => x.AddSingleton(provider));
        return this;
    }

    public PostgresOptions UseCommandsBuilder(IPgCommandsBuilder builder)
    {
        _services.Replace<IPgCommandsBuilder>(x => x.AddSingleton(builder));
        return this;
    }

    public PostgresOptions UseStorageOptions(PgStorageOptions options)
    {
        _services.Replace<PgStorageOptions>(x => x.AddSingleton(options));
        return this;
    }
}