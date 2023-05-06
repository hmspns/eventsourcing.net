using System.Reflection;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.ServiceRegistration;
using EventSourcing.Net.Engine;
using EventSourcing.Net.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net;

/// <summary>
/// Options to configure event sourcing engine.
/// </summary>
public sealed class EventSourcingBusOptions
{
    private readonly EventSourcingOptions _options;

    internal EventSourcingBusOptions(EventSourcingOptions options)
    {
        _options = options;
    }

    internal EventSourcingOptions Options => _options;

    /// <summary>
    /// Register implicit command handlers.
    /// </summary>
    /// <param name="assemblies">Assemblies to register handlers.</param>
    public void RegisterCommandHandlers(params Assembly[] assemblies)
    {
        Type[] types = assemblies
            .SelectMany(x => x.GetTypes())
            .Where(x => x.IsAssignableTo(typeof(ICommandHandler)))
            .ToArray();
        RegisterCommandHandlers(types);
    }

    /// <summary>
    /// Register implicit command handlers.
    /// </summary>
    /// <param name="assembly">Assembly to register handlers.</param>
    public void RegisterCommandHandlers(Assembly assembly)
    {
        Type[] types = assembly.GetTypes().Where(x => x.IsAssignableTo(typeof(ICommandHandler))).ToArray();
        RegisterCommandHandlers(types);
    }

    /// <summary>
    /// Register event consumers.
    /// </summary>
    /// <param name="assembly">Assembly to register consumers.</param>
    public void RegisterEventConsumers(Assembly assembly)
    {
        Type interfaceType = typeof(IEventConsumer<,>);
        Type[] types = assembly.GetTypes().Where(x =>
        {
            IEnumerable<Type> interfaces = x.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
            return interfaces.Any();
        }).ToArray();
        RegisterEventConsumers(types);
    }

    /// <summary>
    /// Register event consumers.
    /// </summary>
    /// <param name="types">Types that implement IEventConsumer interface.</param>
    public void RegisterEventConsumers(Type[] types)
    {
        EventConsumers consumers = new EventConsumers();
        foreach (Type type in types)
        {
            Type[] interfaces = type.GetInterfaces();
            if (interfaces.Any())
            {
                foreach (Type interfaceType in interfaces)
                {
                    MethodInfo methodInfo = interfaceType.GetMethods().First();
                    EventConsumerActivation activation = new EventConsumerActivation()
                    {
                        Method = methodInfo,
                        Type = type
                    };
                    Type argumentType = methodInfo.GetParameters().First().ParameterType;
                    consumers.Add(argumentType, activation);
                }

                _options.Services.Remove(type);
                _options.Services.AddScoped(type);
            }
        }

        Dictionary<Type, EventConsumerActivation[]> results = consumers.GetResults();
        _options.Services.IfNotRegistered<IResolveEventPublisher>(
            services => services.AddSingleton<IResolveEventPublisher>(x => new InMemoryEventPublisherResolver(x, results))
        );
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
                _options.Services.AddTransient(commandHandlerType);
            }
        }

        handlers.TrimExcess();
        _options.Services.IfNotRegistered<IEventSourcingCommandBus>(
            services => services.AddSingleton<IEventSourcingCommandBus>(x => new EventSourcingCommandBus(x, handlers))
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