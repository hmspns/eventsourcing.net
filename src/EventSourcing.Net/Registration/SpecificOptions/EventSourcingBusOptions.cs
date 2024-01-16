using System.Reflection;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.ServiceRegistration;
using EventSourcing.Net.Engine;
using EventSourcing.Net.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net;

using Engine.Exceptions;

/// <summary>
/// Options to configure event sourcing engine.
/// </summary>
public sealed class EventSourcingBusOptions : EventSourcingConfigurationOptions
{
    private HashSet<Type> _eventConsumers = new HashSet<Type>();
    private HashSet<Type> _sagaConsumers = new HashSet<Type>();
    
    private Type _genericEventConsumerType = typeof(IEventConsumer<,>);
    private Type _genericSagaConsumerType = typeof(ISagaConsumer<,>);

    private Func<Type, bool> _isEventConsumer;
    private Func<Type, bool> _isSagaConsumer;
    
    /// <summary>
    /// Initialize new object.
    /// </summary>
    /// <param name="services">Services to provide DI registration.</param>
    internal EventSourcingBusOptions(IServiceCollection services) : base(services)
    {
        _isEventConsumer = x => x.IsGenericType && x.GetGenericTypeDefinition() == _genericEventConsumerType;
        _isSagaConsumer = x => x.IsGenericType && x.GetGenericTypeDefinition() == _genericSagaConsumerType;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _eventConsumers = null;
            _sagaConsumers = null;
            _genericEventConsumerType = null;
            _genericSagaConsumerType = null;
            _isEventConsumer = null;
            _isSagaConsumer = null;
        }
    }

    /// <summary>
    /// Register implicit command handlers.
    /// </summary>
    /// <param name="assemblies">Assemblies to register handlers.</param>
    public void RegisterCommandHandlers(params Assembly[] assemblies)
    {
        CheckDisposed();
        Type[] types = assemblies
            .SelectMany(x => x.GetTypes())
            .Where(x => x.IsAssignableTo(typeof(ICommandHandler)))
            .ToArray();
        RegisterCommandHandlers(types);
    }

    /// <summary>
    /// Register event consumers.
    /// </summary>
    /// <param name="assemblies">Assemblies to register event consumers.</param>
    public void RegisterEventConsumers(params Assembly[] assemblies)
    {
        CheckDisposed();
        IEnumerable<Type> types = GetTypesFromAssemblies(assemblies, _isEventConsumer);
        RegisterEventConsumers(types);
    }

    /// <summary>
    /// Register event consumers.
    /// </summary>
    /// <param name="types">Types that implement IEventConsumer interface.</param>
    public void RegisterEventConsumers(IEnumerable<Type> types)
    {
        CheckDisposed();
        foreach (Type type in types)
        {
            _eventConsumers.Add(type);
        }
    }

    /// <summary>
    /// Register saga consumers.
    /// </summary>
    /// <param name="assemblies">Assemblies to register saga consumers.</param>
    public void RegisterSagaConsumers(params Assembly[] assemblies)
    {
        CheckDisposed();
        IEnumerable<Type> types = GetTypesFromAssemblies(assemblies, _isSagaConsumer);
        RegisterSagaConsumers(types);
    }

    /// <summary>
    /// Register saga consumers.
    /// </summary>
    /// <param name="types">Types that implement ISagaConsumer interface.</param>
    public void RegisterSagaConsumers(IEnumerable<Type> types)
    {
        CheckDisposed();
        foreach (Type type in types)
        {
            _sagaConsumers.Add(type);
        }
    }

    internal void RegisterEventAndSagaConsumersInternal()
    {
        EventConsumers consumers = new EventConsumers();
        foreach (Type type in _eventConsumers.Union(_sagaConsumers))
        {
            Type[] interfaces = type.GetInterfaces();
            if (interfaces.Any())
            {
                foreach (Type interfaceType in interfaces.Where(x => _isEventConsumer(x) || _isSagaConsumer(x)))
                {
                    MethodInfo methodInfo = interfaceType.GetMethods()[0];
                    EventConsumerActivation activation = new EventConsumerActivation()
                    {
                        Method = methodInfo,
                        Type = type
                    };
                    Type argumentType = methodInfo.GetParameters()[0].ParameterType;
                    consumers.Add(argumentType, activation);
                }

                ReplaceScoped(type);
            }
        }

        Dictionary<Type, EventConsumerActivation[]> results = consumers.GetResults();
        IfNotRegistered<IResolveEventPublisher>(
            services => services.AddSingleton<IResolveEventPublisher>(x => new InMemoryEventPublisherResolver(x, results))
        );
    }

    private IEnumerable<Type> GetTypesFromAssemblies(Assembly[] assemblies, Func<Type, bool> predicate)
    {
        if (assemblies == null)
        {
            Thrown.ArgumentNullException(nameof(assemblies));
        }

        if (assemblies.Length == 0)
        {
            Thrown.ArgumentOutOfRangeException(nameof(assemblies), "At least 1 assembly should be passed");
        }

        return assemblies.SelectMany(x => x.GetTypes()).Where(x =>
        {
            IEnumerable<Type> interfaces = x.GetInterfaces()
                                            .Where(predicate);
            return interfaces.Any();
        });
    }

    private void RegisterCommandHandlers(Type[] types)
    {
        Dictionary<Type, CommandHandlerActivation> handlers = new();

        foreach (Type commandHandlerType in types)
        {
            Type aggregateIdType = commandHandlerType.BaseType.GetGenericArguments()[0];
            Type envelopeType = typeof(ICommandEnvelope<,>).MakeGenericType(aggregateIdType, typeof(ICommand));
            Type returnType = typeof(ICommandExecutionResult<>).MakeGenericType(aggregateIdType);
            Type taskReturnType = typeof(Task<>).MakeGenericType(returnType);

            bool hasHandlers = false;
            foreach (MethodInfo method in commandHandlerType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (method.ReturnType.IsAssignableTo(taskReturnType) && IsValidCommandHandlerMethod(method,
                        envelopeType,
                        out bool useCancellation, out Type commandType))
                {
                    Type dictionaryKeyType = typeof(ICommandEnvelope<,>).MakeGenericType(aggregateIdType, commandType);

                    handlers.Add(dictionaryKeyType, new CommandHandlerActivation()
                    {
                        Method = method,
                        UseCancellation = useCancellation
                    });
                    hasHandlers = true;
                }
            }

            if (hasHandlers)
            {
                ReplaceTransient(commandHandlerType);
            }
        }

        handlers.TrimExcess();
        IfNotRegistered<IEventSourcingCommandBus>(
            services => services.AddSingleton<IEventSourcingCommandBus>(x => new EventSourcingCommandBus(x, handlers))
        );
        IfNotRegistered<ISagaEventSourcingCommandBus>(
            services => services.AddSingleton<ISagaEventSourcingCommandBus>(x =>
                new SagaEventSourcingCommandBus(x.GetRequiredService<IEventSourcingCommandBus>()))
        );
    }

    private static bool IsValidCommandHandlerMethod(MethodInfo methodInfo, Type envelopeType, out bool useCancellation,
                                                    out Type commandType)
    {
        ParameterInfo[] parameters = methodInfo.GetParameters();
        if (parameters.Length == 1 &&
            parameters[0].ParameterType.IsAssignableTo(envelopeType))
        {
            useCancellation = false;
            commandType = GetCommandType();
            return true;
        }

        if (parameters.Length == 2 &&
            parameters[0].ParameterType.IsAssignableTo(envelopeType) &&
            parameters[1].ParameterType == typeof(CancellationToken))
        {
            useCancellation = true;
            commandType = GetCommandType();
            return true;
        }

        useCancellation = false;
        commandType = null;
        return false;

        Type GetCommandType()
        {
            return parameters[0].ParameterType.GenericTypeArguments[1];
        }
    }
}