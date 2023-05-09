using System.Reflection;
using EventSourcing.Net.Samples.Persistence.Data;
using EventSourcing.Net.Storage.Postgres;
using EventSourcing.Net.Storage.Redis;
using Microsoft.EntityFrameworkCore;

namespace EventSourcing.Net.Samples.Persistence;

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
            options.Bus.RegisterCommandHandlers(assembly);
            options.Bus.RegisterEventConsumers(assembly);
            options.Storage.UsePostgresEventStore(configuration.GetConnectionString("EventsDb"))
                .Configure(x =>
                {
                    x.UseMultitenancy = true;
                    x.StoreTenantId = false;
                    x.StoreCommands = true;
                    x.StorePrincipal = false;
                    x.StoreCommandSource = false;
                });
            options.Storage.UseRedisSnapshotStore(configuration.GetConnectionString("Redis"), new RedisSnapshotCreationPolicy()
            {
                Behaviour = RedisSnapshotCreationBehaviour.ThresholdCommit,
                CommitThreshold = 10,
                ExpireAfter = TimeSpan.FromMinutes(5)
            });
        });

        return services;
    }

    public static async Task CreateDatabases(this WebApplication serviceProvider)
    {
        await using var scope = serviceProvider.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreatedAsync();
        await scope.ServiceProvider.GetRequiredService<EventsDbContext>().Database.EnsureCreatedAsync();
    }
}