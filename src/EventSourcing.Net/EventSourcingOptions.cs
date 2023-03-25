using System.Reflection;
using EventSourcing.Abstractions;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Core;
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
        _services.AddTransient<IResolveEventPublisher, NoEventPublisherResolver>();
    }

    internal void Remove(Type interfaceType)
    {
        ServiceDescriptor? descriptor = _services.FirstOrDefault(x => x.ServiceType == interfaceType);
        if (descriptor != null)
        {
            _services.Remove(descriptor);
        }
    }
    
        /// <summary>
    /// Register implicit command handlers.
    /// </summary>
    /// <param name="services">Services.</param>
    /// <param name="assembly">Assembly to register handlers.</param>
    public void RegisterImplicitHandlers(Assembly assembly)
    {
        Dictionary<Type, CommandHandlerActivation> handlers = new();

        Type[] types = assembly.GetTypes().Where(x => x.IsAssignableTo(typeof(ICommandHandler))).ToArray();
        foreach (Type type in types)
        {
            Type aggregateIdType = type.BaseType.GetGenericArguments()[0];
            Type envelopeType = typeof(ICommandEnvelope<,>).MakeGenericType(aggregateIdType, typeof(ICommand));
            Type returnType = typeof(ICommandExecutionResult<>).MakeGenericType(aggregateIdType);
            Type taskReturnType = typeof(Task<>).MakeGenericType(returnType);

            bool hasHandlers = false;
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (method.ReturnType.IsAssignableTo(taskReturnType) && IsValidMethod(method, envelopeType, out bool useCancellation, out Type commandType))
                {
                    Type dictionaryType = typeof(ICommandEnvelope<,>).MakeGenericType(aggregateIdType, commandType);
                    handlers.Add(dictionaryType, new CommandHandlerActivation()
                    {
                        Method = method,
                        UseCancellation = useCancellation
                    });
                    hasHandlers = true;
                }
            }

            if (hasHandlers)
            {
                _services.AddTransient(type);
            }
        }
        Remove(typeof(IEventSourcingCommandBus));
        _services.AddTransient<IEventSourcingCommandBus, InMemoryCommandBus>();
        InMemoryCommandBus.Initialize(handlers);
    }

    private static bool IsValidMethod(MethodInfo methodInfo, Type envelopeType, out bool useCancellation, out Type commandType)
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
