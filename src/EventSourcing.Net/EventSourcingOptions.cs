using EventSourcing.Abstractions;
using EventSourcing.Core.Contracts;
using EventSourcing.Core.Implementations;
using EventSourcing.Core.InMemory;
using EventSourcing.Core.NoImplementation;
using EventSourcing.Core.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EventSourcing.Net;

public sealed class EventSourcingOptions
{
    internal readonly IServiceCollection _services;
    
    internal EventSourcingOptions(IServiceCollection services)
    {
        _services = services;
        



        // services.AddTransient<IEventSourcingCommandBus, EventSourcingCommandBus>();
        // services.AddTransient<IResolveEventPublisher, EventPublisherResolver>();
        // services.AddTransient<IResolveEventStore, EventStoreResolver>();
        // services.AddTransient<IResolveSnapshotStore, SnapshotStoreResolver>();
        // services.AddTransient<IEventSourcingEngine, EventSourcingEngine>();
        // services.AddTransient<IPayloadSerializer, JsonPayloadSerializer>();
        //
        // services.AddTransient<IResolveAppender, PgAppenderResolver>(
        //     provider => new PgAppenderResolver(
        //         connectionString, provider.GetRequiredService<IPayloadSerializer>()));
        //
        // // должен быть singleton
        // services.AddSingleton<IPublicationAwaiter, MediatorPublicationAwaiter>();
    }

    internal void Build()
    {
        _services.AddTransient<IResolveEventStore, EventStoreResolver>();
        _services.AddTransient<IResolveSnapshotStore, NoSnapshotStoreResolver>();
        _services.AddTransient<IEventSourcingEngine, EventSourcingEngine>();
        _services.AddSingleton<IResolveAppender, InMemoryResolveAppender>();
        _services.AddTransient<IPayloadSerializer, SystemTextJsonPayloadSerializer>();
        _services.AddSingleton<ILoggerFactory, NoLoggerFactory>();
    }
}