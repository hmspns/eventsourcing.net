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

    private IEventTypeMappingHandler _eventTypeMappingHandler;

    internal EventSourcingOptions(IServiceCollection services)
    {
        _services = services;
        _busOptions = new EventSourcingBusOptions(services);
    }

    /// <summary>
    /// Get object to configure bus.
    /// </summary>
    public EventSourcingBusOptions Bus => _busOptions;

    /// <summary>
    /// Register classes that implement IEvent for serialization mapping from given assemblies.
    /// </summary>
    /// <param name="assemblies">Assemblies to search types.</param>
    /// <returns>EventSourcingOptions for next fluent call.</returns>
    /// <exception cref="ArgumentNullException">Assemblies is null.</exception>
    public EventSourcingOptions RegisterEventTypesMapping(params Assembly[] assemblies)
    {
        if (assemblies == null)
        {
            throw new ArgumentNullException(nameof(assemblies));
        }

        if (assemblies.Length == 0)
        {
            assemblies = AppDomain.CurrentDomain.GetAssemblies();
        }
        Dictionary<string,Type> mappings = assemblies.SelectMany(x => x.GetTypes())
            .Where(x => x.IsAssignableTo(typeof(IEvent)))
            .ToDictionary(x => x.FullName, x => x, StringComparer.Ordinal);
        mappings.TrimExcess();
        _eventTypeMappingHandler = new InMemoryEventTypeMappingHandler(mappings);
        return this;
    }

    /// <summary>
    /// Register custom IEventTypeMappingHandler.
    /// </summary>
    /// <param name="handler">Instance of IEventTypeMappingHandler.</param>
    /// <returns>EventSourcingOptions for next fluent call.</returns>
    /// <remarks>Handler will be registered as singleton.</remarks>
    /// <exception cref="ArgumentNullException">Handler is null.</exception>
    public EventSourcingOptions RegisterEventTypesMapping(IEventTypeMappingHandler handler)
    {
        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }
        _eventTypeMappingHandler = handler;
        return this;
    }

    internal void Build()
    {
        _services.AddTransient<IResolveEventStore, EventStoreResolver>();
        _services.AddTransient<IResolveSnapshotStore, NoSnapshotStoreResolver>();
        _services.AddTransient<IEventSourcingEngine, EventSourcingEngine>();
        _services.AddSingleton<IResolveAppender, InMemoryResolveAppender>();
        _services.AddSingleton<IEventsPayloadSerializerFactory, SystemTextJsonEventsSerializerFactory>();
        _services.AddSingleton<ISnapshotsSerializerFactory, SystemTextJsonSnapshotsSerializerFactory>();
        
         if (_eventTypeMappingHandler == null)
         {
             RegisterEventTypesMapping();
         }
         _services.AddSingleton<IEventTypeMappingHandler>(_eventTypeMappingHandler);
    }
}