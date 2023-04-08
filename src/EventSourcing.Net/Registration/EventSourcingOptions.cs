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
    internal IServiceCollection Services { get; private set; }
    
    private readonly EventSourcingBusOptions _busOptions;

    private IEventTypeMappingHandler _eventTypeMappingHandler;

    internal EventSourcingOptions(IServiceCollection services)
    {
        Services = services;
        _busOptions = new EventSourcingBusOptions(this);
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
        Services.IfNotRegistered<IEventsPayloadSerializerFactory>(x => x.AddSingleton<IEventsPayloadSerializerFactory, SystemTextJsonEventsSerializerFactory>());
        Services.IfNotRegistered<ISnapshotsSerializerFactory>(x => x.AddSingleton<ISnapshotsSerializerFactory, SystemTextJsonSnapshotsSerializerFactory>());
        
         if (_eventTypeMappingHandler == null)
         {
             RegisterEventTypesMapping();
         }

         Services.IfNotRegistered<IEventTypeMappingHandler>(x => x.AddSingleton<IEventTypeMappingHandler>(_eventTypeMappingHandler));
         
         RegisterEventSourcingEngine();
         Services = null; // do not handle reference to service collection
    }

    private void RegisterEventSourcingEngine()
    {
        Services.IfNotRegistered<IResolveEventStore>(x => x.AddSingleton<IResolveEventStore, EventStoreResolver>());
        Services.IfNotRegistered<IResolveSnapshotStore>(x => x.AddSingleton<IResolveSnapshotStore, NoSnapshotStoreResolver>());
        Services.IfNotRegistered<IResolveAppender>(x => x.AddSingleton<IResolveAppender, InMemoryResolveAppender>());
        Services.IfNotRegistered<IResolveEventPublisher>(x => x.AddSingleton<IResolveEventPublisher, NoEventPublisherResolver>());
        
        IServiceCollection local = Services;
        Lazy<IEventSourcingEngine> lazy = new Lazy<IEventSourcingEngine>(() =>
        {
            ServiceProvider provider = local.BuildServiceProvider();
            EventSourcingEngine engine = new EventSourcingEngine(
                provider.GetRequiredService<IResolveEventStore>(),
                provider.GetRequiredService<IResolveSnapshotStore>(),
                provider.GetRequiredService<IResolveEventPublisher>());
            return engine;
        }, LazyThreadSafetyMode.ExecutionAndPublication);
        
        EventSourcingEngineFactory.Initialize(lazy);

        Services.IfNotRegistered<IEventSourcingEngine>(x => x.AddSingleton<IEventSourcingEngine>(lazy.Value));
        
        
    }
    
    
    private static Type[] GetTypesForMapping(params Assembly[] assemblies)
    {
        Type[] allTypes = assemblies.SelectMany(x => x.GetTypes()).ToArray();

        IEnumerable<Type> events = allTypes.Where(x => x.IsAssignableTo(typeof(IEvent)));
        IEnumerable<Type> commands = allTypes.Where(x => x.IsAssignableTo(typeof(ICommand)));

        List<Type> aggregateParts = new List<Type>();
        IEnumerable<Type> aggregates = allTypes.Where(x => x.IsAssignableTo(typeof(IAggregate)));
        foreach (Type aggregateType in aggregates)
        {
            if (aggregateType.BaseType is { ContainsGenericParameters: true, GenericTypeArguments.Length: 3, })
            {
                Type genericTypeDefinition = aggregateType.BaseType.GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(Aggregate<,,>))
                {
                    Type[] args = genericTypeDefinition.GetGenericArguments();
                    aggregateParts.Add(args[0]); // TId
                    aggregateParts.Add(args[1]); // TState
                }
            }
        }

        return aggregateParts.Union(events).Union(commands).Distinct().ToArray();
    }
}