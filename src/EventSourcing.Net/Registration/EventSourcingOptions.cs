using System.Reflection;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Core;
using EventSourcing.Core.Implementations;
using EventSourcing.Core.InMemory;
using EventSourcing.Core.NoImplementation;
using EventSourcing.Core.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net;

public sealed class EventSourcingOptions
{
    internal readonly IServiceCollection _services;

    private readonly EventSourcingBusOptions _busOptions;

    internal EventSourcingOptions(IServiceCollection services)
    {
        _services = services;
        _busOptions = new EventSourcingBusOptions(services);
    }

    /// <summary>
    /// Get object to configure bus.
    /// </summary>
    public EventSourcingBusOptions Bus => _busOptions;

    internal void Build()
    {
        _services.AddTransient<IResolveEventStore, EventStoreResolver>();
        _services.AddTransient<IResolveSnapshotStore, NoSnapshotStoreResolver>();
        _services.AddTransient<IEventSourcingEngine, EventSourcingEngine>();
        _services.AddSingleton<IResolveAppender, InMemoryResolveAppender>();
        _services.AddTransient<IPayloadSerializer, SystemTextJsonPayloadSerializer>();
    }
}