using System.Reflection;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Net;
using EventSourcing.Storage.Postgres;
using EventSourcing.Storage.Redis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventSouring.Benchmark.General;

public static class BenchmarkBus
{
    private static readonly IConfigurationRoot _configuration;
    private static readonly Assembly _assembly;
    
    public static TenantId TenantId { get; } = TenantId.New();

    private static readonly RedisSnapshotCreationPolicy _redisPolicy = new RedisSnapshotCreationPolicy()
    {
        Behaviour = RedisSnapshotCreationBehaviour.ThresholdCommit,
        CommitThreshold = 10,
        ExpireAfter = TimeSpan.FromMinutes(5)
    };

    public static (IEventSourcingCommandBus, IEventSourcingEngine) InMemoryBus { get; private set; }
    
    public static (IEventSourcingCommandBus, IEventSourcingEngine) PgFullRedisBus { get; private set; }
    
    public static (IEventSourcingCommandBus, IEventSourcingEngine) PgMinRedisBus { get; private set; }
    
    public static (IEventSourcingCommandBus, IEventSourcingEngine) PgFullNoRedisBus { get; private set; }
    
    public static (IEventSourcingCommandBus, IEventSourcingEngine) PgMinNoRedisBus { get; private set; }
    
    static BenchmarkBus()
    {
        _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        _assembly = Assembly.GetExecutingAssembly();

        CreateDb();
        InitializeInMemory();
        InitializePgFullRedisBus();
        InitializePgFullNoRedisBus();
        InitializePgMinRedisBus();
        InitializePgMinNoRedisBus();
    }

    private static void CreateDb()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddDbContext<EventsDbContext>(x => x.UseNpgsql(_configuration.GetConnectionString("EventsDb")));
        var provider = services.BuildServiceProvider();

        var context = provider.GetRequiredService<EventsDbContext>();
        context.Database.EnsureCreated();
    }

    private static void InitializeInMemory()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddEventSourcing(options =>
        {
            options.Bus.RegisterImplicitCommandHandlers(_assembly);
            options.Bus.RegisterEventConsumers(_assembly);
        });
        ServiceProvider provider = services.BuildServiceProvider();
        InMemoryBus = (provider.GetRequiredService<IEventSourcingCommandBus>(), provider.GetRequiredService<IEventSourcingEngine>());
    }

    private static void InitializePgFullRedisBus()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddEventSourcing(options =>
        {
            options.Bus.RegisterImplicitCommandHandlers(_assembly);
            options.Bus.RegisterEventConsumers(_assembly);
            options.UsePostgresEventsStore(_configuration.GetConnectionString("EventsDb"))
                .Configure(x =>
                {
                    x.UseMultitenancy = true;
                    x.StoreTenantId = true;
                    x.StoreCommands = true;
                    x.StorePrincipal = true;
                    x.StoreCommandSource = true;
                    x.MultitenancySchemaName = x => "full_redis_" + x.ToString();
                });
            options.UseRedisSnapshotStore(_configuration.GetConnectionString("Redis"), _redisPolicy);
        });
        ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredService<IResolveAppender>().Get(TenantId).Initialize().Wait();
        PgFullRedisBus = (provider.GetRequiredService<IEventSourcingCommandBus>(), provider.GetRequiredService<IEventSourcingEngine>());
    }
    
    private static void InitializePgFullNoRedisBus()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddEventSourcing(options =>
        {
            options.Bus.RegisterImplicitCommandHandlers(_assembly);
            options.Bus.RegisterEventConsumers(_assembly);
            options.UsePostgresEventsStore(_configuration.GetConnectionString("EventsDb"))
                .Configure(x =>
                {
                    x.UseMultitenancy = true;
                    x.StoreTenantId = true;
                    x.StoreCommands = true;
                    x.StorePrincipal = true;
                    x.StoreCommandSource = true;
                    x.MultitenancySchemaName = x => "full_no_redis_" + x.ToString();
                });
        });
        ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredService<IResolveAppender>().Get(TenantId).Initialize().Wait();
        PgFullNoRedisBus = (provider.GetRequiredService<IEventSourcingCommandBus>(), provider.GetRequiredService<IEventSourcingEngine>());
    }

    private static void InitializePgMinRedisBus()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddEventSourcing(options =>
        {
            options.Bus.RegisterImplicitCommandHandlers(_assembly);
            options.Bus.RegisterEventConsumers(_assembly);
            options.UsePostgresEventsStore(_configuration.GetConnectionString("EventsDb"))
                .Configure(x =>
                {
                    x.UseMultitenancy = false;
                    x.StoreTenantId = false;
                    x.StoreCommands = false;
                    x.StorePrincipal = false;
                    x.StoreCommandSource = false;
                    x.NonMultitenancySchemaName = "min_redis";
                });
            options.UseRedisSnapshotStore(_configuration.GetConnectionString("Redis"), _redisPolicy);
        });
        ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredService<IResolveAppender>().Get(TenantId).Initialize().Wait();
        PgMinRedisBus = (provider.GetRequiredService<IEventSourcingCommandBus>(), provider.GetRequiredService<IEventSourcingEngine>());
    }
    
    private static void InitializePgMinNoRedisBus()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddEventSourcing(options =>
        {
            options.Bus.RegisterImplicitCommandHandlers(_assembly);
            options.Bus.RegisterEventConsumers(_assembly);
            options.UsePostgresEventsStore(_configuration.GetConnectionString("EventsDb"))
                .Configure(x =>
                {
                    x.UseMultitenancy = false;
                    x.StoreTenantId = false;
                    x.StoreCommands = false;
                    x.StorePrincipal = false;
                    x.StoreCommandSource = false;
                    x.NonMultitenancySchemaName = "min_no_redis";
                });
        });
        ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredService<IResolveAppender>().Get(TenantId).Initialize().Wait();
        PgMinNoRedisBus = (provider.GetRequiredService<IEventSourcingCommandBus>(), provider.GetRequiredService<IEventSourcingEngine>());
    }
}

/// <summary>
/// It's using to create events db only.
/// </summary>
public sealed class EventsDbContext : DbContext
{
    public EventsDbContext(DbContextOptions<EventsDbContext> options) : base(options)
    {
        
    }
}