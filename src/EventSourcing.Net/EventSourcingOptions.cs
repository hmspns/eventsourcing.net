using EventSourcing.Abstractions;
using EventSourcing.Abstractions.Contracts;
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
    }

    internal void Build()
    {
        _services.AddTransient<IResolveEventStore, EventStoreResolver>();
        _services.AddTransient<IResolveSnapshotStore, NoSnapshotStoreResolver>();
        _services.AddTransient<IEventSourcingEngine, EventSourcingEngine>();
        _services.AddSingleton<IResolveAppender, InMemoryResolveAppender>();
        _services.AddTransient<IPayloadSerializer, SystemTextJsonPayloadSerializer>();
        _services.AddTransient<IEventSourcingCommandBus, InMemoryCommandBus>();
    }

    internal void Remove(Type interfaceType)
    {
        ServiceDescriptor? descriptor = _services.FirstOrDefault(x => x.ServiceType == interfaceType);
        if (descriptor != null)
        {
            _services.Remove(descriptor);
        }
    }
}