using System.Reflection;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net;
using EventSourcing.Net.Storage.Postgres;
using EventSourcing.Net.Storage.Redis;
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
        MinAggregateVersion = 100,
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
            options.Bus.RegisterCommandHandlers(_assembly);
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
            options.Bus.RegisterCommandHandlers(_assembly);
            options.Bus.RegisterEventConsumers(_assembly);
            options.Storage.UsePostgresEventStore(_configuration.GetConnectionString("EventsDb"))
                .Configure(x =>
                {
                    x.UseMultitenancy = true;
                    x.StoreTenantId = true;
                    x.StoreCommands = true;
                    x.StorePrincipal = true;
                    x.StoreCommandSource = true;
                    x.MetadataSchemaName = "PgFullRedis";
                    x.MultitenancySchemaName = x => "full_redis_" + x.ToString();
                });
            options.Storage.UseRedisSnapshotStore(_configuration.GetConnectionString("Redis"), _redisPolicy);
        });
        ServiceProvider provider = services.BuildServiceProvider();
        InitializeEs(provider);
        PgFullRedisBus = (provider.GetRequiredService<IEventSourcingCommandBus>(), provider.GetRequiredService<IEventSourcingEngine>());
    }
    
    private static void InitializePgFullNoRedisBus()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddEventSourcing(options =>
        {
            options.Bus.RegisterCommandHandlers(_assembly);
            options.Bus.RegisterEventConsumers(_assembly);
            options.Storage.UsePostgresEventStore(_configuration.GetConnectionString("EventsDb"))
                .Configure(x =>
                {
                    x.UseMultitenancy = true;
                    x.StoreTenantId = true;
                    x.StoreCommands = true;
                    x.StorePrincipal = true;
                    x.StoreCommandSource = true;
                    x.MetadataSchemaName = "PgFullNoRedis";
                    x.MultitenancySchemaName = x => "full_no_redis_" + x.ToString();
                });
        });
        ServiceProvider provider = services.BuildServiceProvider();
        InitializeEs(provider);
        PgFullNoRedisBus = (provider.GetRequiredService<IEventSourcingCommandBus>(), provider.GetRequiredService<IEventSourcingEngine>());
    }

    private static void InitializePgMinRedisBus()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddEventSourcing(options =>
        {
            options.Bus.RegisterCommandHandlers(_assembly);
            options.Bus.RegisterEventConsumers(_assembly);
            options.Storage.UsePostgresEventStore(_configuration.GetConnectionString("EventsDb"))
                .Configure(x =>
                {
                    x.UseMultitenancy = false;
                    x.StoreTenantId = false;
                    x.StoreCommands = false;
                    x.StorePrincipal = false;
                    x.StoreCommandSource = false;
                    x.MetadataSchemaName = "PgMinRedis";
                    x.NonMultitenancySchemaName = "min_redis";
                });
            options.Storage.UseRedisSnapshotStore(_configuration.GetConnectionString("Redis"), _redisPolicy);
        });
        ServiceProvider provider = services.BuildServiceProvider();
        InitializeEs(provider);
        PgMinRedisBus = (provider.GetRequiredService<IEventSourcingCommandBus>(), provider.GetRequiredService<IEventSourcingEngine>());
    }
    
    private static void InitializePgMinNoRedisBus()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddEventSourcing(options =>
        {
            options.Bus.RegisterCommandHandlers(_assembly);
            options.Bus.RegisterEventConsumers(_assembly);
            options.Storage.UsePostgresEventStore(_configuration.GetConnectionString("EventsDb"))
                .Configure(x =>
                {
                    x.UseMultitenancy = false;
                    x.StoreTenantId = false;
                    x.StoreCommands = false;
                    x.StorePrincipal = false;
                    x.StoreCommandSource = false;
                    x.MetadataSchemaName = "PgMinNoRedis";
                    x.NonMultitenancySchemaName = "min_no_redis";
                });
        });
        ServiceProvider provider = services.BuildServiceProvider();
        InitializeEs(provider);
        PgMinNoRedisBus = (provider.GetRequiredService<IEventSourcingCommandBus>(), provider.GetRequiredService<IEventSourcingEngine>());
    }

    private static void InitializeEs(IServiceProvider provider)
    {
        provider.StartEventSourcingEngine().Wait();
        provider.GetRequiredService<IEventSourcingStorage>().Initialize(TenantId).Wait();
    }
}