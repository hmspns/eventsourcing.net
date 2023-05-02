using System.Reflection;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Core;
using EventSourcing.Core.Implementations;
using EventSourcing.Core.InMemory;
using EventSourcing.Core.NoImplementation;
using EventSourcing.Core.Serialization;
using EventSourcing.Net.Abstractions.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net;

public sealed class EventSourcingOptions
{
    internal IServiceCollection Services { get; private set; }
    
    private readonly EventSourcingBusOptions _busOptions;

    private ITypeStringConverter _typeStringConverter;

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

        Type[] types = GetTypesForMapping(assemblies);
        Services.AddSingleton<ITypeMappingHandler>(x =>
        {
            ITypeMappingStorageProvider typeMappingStorageProvider = x.GetRequiredService<ITypeMappingStorageProvider>();
            ITypeStringConverter typeStringConverter = x.GetRequiredService<ITypeStringConverter>();
            TypeMappingHandler handler = new TypeMappingHandler(typeMappingStorageProvider, typeStringConverter);
            ((ITypeMappingHandler)handler).SetStorageTypes(types);
            return handler;
        });

        _typeStringConverter = new DefaultTypeStringConverter();
        Services.AddSingleton(_typeStringConverter);
        return this;
    }

    /// <summary>
    /// Register custom IEventTypeMappingHandler.
    /// </summary>
    /// <param name="handler">Instance of IEventTypeMappingHandler.</param>
    /// <returns>EventSourcingOptions for next fluent call.</returns>
    /// <remarks>Handler will be registered as singleton.</remarks>
    /// <exception cref="ArgumentNullException">Handler is null.</exception>
    public EventSourcingOptions RegisterEventTypesMapping(ITypeStringConverter handler)
    {
        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }
        _typeStringConverter = handler;
        return this;
    }

    internal void Build()
    {
        Services.AddSingleton<EventSourcingEngineStarter>();
        Services.IfNotRegistered<IEventsPayloadSerializerFactory>(x => x.AddSingleton<IEventsPayloadSerializerFactory, SystemTextJsonEventsSerializerFactory>());
        Services.IfNotRegistered<ISnapshotsSerializerFactory>(x => x.AddSingleton<ISnapshotsSerializerFactory, SystemTextJsonSnapshotsSerializerFactory>());
        
        if (_typeStringConverter == null)
        { 
            RegisterEventTypesMapping();
        }

        Services.IfNotRegistered<ITypeStringConverter>(x => x.AddSingleton<ITypeStringConverter>(_typeStringConverter));

        Services.IfNotRegistered<ITypeMappingStorageProvider>(x => x.AddSingleton<ITypeMappingStorageProvider, InMemoryTypeMappingStorageProvider>());
        Services.IfNotRegistered<IEventSourcingStorage>(x => x.AddTransient<IEventSourcingStorage, InMemoryEventSourcingStorage>());
        
        RegisterEventSourcingEngine();
        Services = null; // do not handle reference to the service collection
    }

    private void RegisterEventSourcingEngine()
    {
        Services.IfNotRegistered<IResolveEventStore>(x => x.AddSingleton<IResolveEventStore, EventStoreResolver>());
        Services.IfNotRegistered<IResolveSnapshotStore>(x => x.AddSingleton<IResolveSnapshotStore, NoSnapshotStoreResolver>());
        Services.IfNotRegistered<IResolveAppender>(x => x.AddSingleton<IResolveAppender, InMemoryResolveAppender>());
        Services.IfNotRegistered<IResolveEventPublisher>(x => x.AddSingleton<IResolveEventPublisher, NoEventPublisherResolver>());
        Services.IfNotRegistered<IEventSourcingEngine>(x => x.AddSingleton<IEventSourcingEngine, EventSourcingEngine>());
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
            if (aggregateType.BaseType is { IsGenericType: true, GenericTypeArguments.Length: 3, })
            {
                Type genericTypeDefinition = aggregateType.BaseType.GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(Aggregate<,,>))
                {
                    Type[] args = aggregateType.BaseType.GetGenericArguments();
                    aggregateParts.Add(args[0]); // TId
                    aggregateParts.Add(args[1]); // TState
                }
            }
        }

        return aggregateParts.Union(events).Union(commands).Distinct().Where(x => !x.IsInterface).ToArray();
    }
}