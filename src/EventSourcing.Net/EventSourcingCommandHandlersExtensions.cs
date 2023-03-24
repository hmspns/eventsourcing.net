using System.Reflection;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Core;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net;

public static class EventSourcingCommandHandlersExtensions
{
    public static void RegisterImplicitHandlers(this IServiceCollection services, Assembly assembly)
    {
        Dictionary<Type, MethodInfo> handlers = new();

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
                if (method.ReturnType.IsAssignableTo(taskReturnType))
                {
                    var handler = TryRegisterWithoutCancellation(type, envelopeType, taskReturnType, method)
                    if (handler == null)
                    {
                        handler = TryRegisterWithCancellation(envelopeType, method);
                    }
                    
                    if (handler != null)
                    {
                        handlers.Add(envelopeType, handler);
                        hasHandlers = true;
                    }
                }
            }

            if (hasHandlers)
            {
                services.AddTransient(type);
            }
        }
    }

    private static Func<ICommandEnvelope, CancellationToken, object>? TryRegisterWithoutCancellation(
        Type commandHandlerType,
        Type envelopeType, 
        Type returnType,
        MethodInfo methodInfo)
    {
        ParameterInfo[] parameters = methodInfo.GetParameters();
        if (parameters.Length == 1 && parameters[0].ParameterType.IsAssignableTo(envelopeType))
        {
            Func<ICommandEnvelope, CancellationToken, object> handler = (envelope, token) =>
            {
                var handler = Activator.CreateInstance(commandHandlerType);
                var result = methodInfo.Invoke()
                return null;
            };
            return handler;
        }

        return null;
    }

    private static Func<ICommandEnvelope, CancellationToken, object>? TryRegisterWithCancellation(Type parameterType, MethodInfo methodInfo)
    {
        ParameterInfo[] parameters = methodInfo.GetParameters();
        if (parameters.Length == 2 &&
            parameters[0].ParameterType.IsAssignableTo(parameterType) &&
            parameters[1].ParameterType == typeof(CancellationToken))
        {
        }
    }
}