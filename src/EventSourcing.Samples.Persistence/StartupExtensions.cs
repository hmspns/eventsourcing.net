using System.Reflection;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Net;
using EventSourcing.Samples.Persistence.Data;
using EventSourcing.Storage.Postgres;
using EventSourcing.Storage.Redis;
using Microsoft.EntityFrameworkCore;

namespace EventSourcing.Samples.Persistence;

public static class StartupExtensions
{
    public static IServiceCollection RegisterDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("ViewsDb"));
        });
        services.AddDbContext<EventsDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("EventsDb"));
        });

        return services;
    }

    public static IServiceCollection RegisterEventSourcing(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEventSourcing(options =>
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            options.Bus.RegisterImplicitCommandHandlers(assembly);
            options.Bus.RegisterEventConsumers(assembly);
            options.UsePostgresEventsStore(configuration.GetConnectionString("EventsDb"));
            //options.UseRedisSnapshotStore(configuration.GetConnectionString("Redis"));
        });

        return services;
    }

    public static async Task CreateDatabases(this WebApplication serviceProvider)
    {
        await using var scope = serviceProvider.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreatedAsync();
        await scope.ServiceProvider.GetRequiredService<EventsDbContext>().Database.EnsureCreatedAsync();
        // this will create schema for tenant
        await scope.ServiceProvider.GetRequiredService<IResolveAppender>().Get(TenantId.Empty).Initialize();
    }
}