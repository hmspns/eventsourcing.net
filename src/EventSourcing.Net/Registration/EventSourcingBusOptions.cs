using System.Reflection;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.ServiceRegistration;
using EventSourcing.Core;
using EventSourcing.Net.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net;

public sealed class EventSourcingBusOptions
{
    private readonly IServiceCollection _services;

    internal EventSourcingBusOptions(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Register implicit command handlers.
    /// </summary>
    /// <param name="services">Services.</param>
    /// <param name="assembly">Assemblies to register handlers.</param>
    public void RegisterImplicitCommandHandlers(params Assembly[] assemblies)
    {
        Type[] types = assemblies
            .SelectMany(x => x.GetTypes())
            .Where(x => x.IsAssignableTo(typeof(ICommandHandler)))
            .ToArray();
        RegisterImplicitCommandHandlers(types);
    }

    /// <summary>
    /// Register implicit command handlers.
    /// </summary>
    /// <param name="services">Services.</param>
    /// <param name="assembly">Assembly to register handlers.</param>
    public void RegisterImplicitCommandHandlers(Assembly assembly)
    {
        Type[] types = assembly.GetTypes().Where(x => x.IsAssignableTo(typeof(ICommandHandler))).ToArray();
        RegisterImplicitCommandHandlers(types);
    }

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

    private void RegisterEventConsumers(Type[] types)
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

                _services.Remove(type);
                _services.AddTransient(type);
            }
        }

        var results = consumers.GetResults();
        _services.Remove(typeof(IResolveEventPublisher));
        _services.AddSingleton<IResolveEventPublisher>(x =>
        {
            return new InMemoryEventPublisherResolver(x, results);
        });
    }
    
    private void RegisterImplicitCommandHandlers(Type[] types)
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
                _services.AddTransient(commandHandlerType);
            }
        }

        handlers.TrimExcess();
        _services.Remove(typeof(IEventSourcingCommandBus));
        _services.AddSingleton<IEventSourcingCommandBus>(x => new InMemoryCommandBus(x, handlers));
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