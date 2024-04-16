using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Engine;
using EventSourcing.Net.Engine.Exceptions;
using EventSourcing.Net.Internal;
using Microsoft.Extensions.DependencyInjection;

#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

namespace EventSourcing.Net;

using System.Linq.Expressions;
using System.Reflection;

/// <summary>
/// Built in command bus.
/// </summary>
public sealed class CallByDelegateEventSourcingCommandBus : IEventSourcingCommandBus
{
    /// <inheritdoc />
    public IPublicationAwaiter PublicationAwaiter => _publicationAwaiter;
    
    private readonly IReadOnlyDictionary<Type, ByDelegateActivation> _handlers;
    private readonly IServiceProvider _provider;
    private readonly InMemoryPublicationAwaiter _publicationAwaiter = new();

    internal CallByDelegateEventSourcingCommandBus(IServiceProvider provider, IReadOnlyDictionary<Type, CommandHandlerActivation> handlers)
    {
        _provider = provider;

        _handlers = handlers.ToDictionary(x => x.Key, x => CreateDelegateActivation(x.Value));
        
#if NET8_0_OR_GREATER
        _handlers = _handlers.ToFrozenDictionary();
#endif
    }

    /// <summary>
    /// Send command to handler.
    /// </summary>
    /// <param name="tenantId">Tenant id.</param>
    /// <param name="principalId">Id of principal.</param>
    /// <param name="source">Command source.</param>
    /// <param name="aggregateId">Aggregate id.</param>
    /// <param name="commandPayload">Command payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TId">Type of aggregate id.</typeparam>
    /// <typeparam name="TPayload">Type of command payload.</typeparam>
    /// <returns>Result of command execution.</returns>
    /// <exception cref="InvalidOperationException">Handler not registered.</exception>
    /// <remarks>
    /// Command source is the place where command was sent.
    /// 
    /// It's important to have a good performance use a specific type of TPayload instead of general ICommand.
    /// When ICommand passed as TPayload bus has to use reflection to find the proper handler and create command envelope.
    /// When a specific type passed as TPayload reflection not needed.
    /// </remarks>
    public Task<ICommandExecutionResult<TId>> Send<TId, TPayload>(TenantId tenantId, PrincipalId principalId, string source,
        TId aggregateId, TPayload commandPayload, CancellationToken cancellationToken = default) where TPayload : ICommand
    {
        ICommandEnvelope<TId> command = CommandEnvelopeBuilder.ToEnvelope(tenantId, principalId, source, aggregateId, commandPayload);
        return Send<TId, TPayload>(command, cancellationToken);
    }

    /// <summary>
    /// Send command to handler.
    /// </summary>
    /// <param name="commandEnvelope">Command envelope that will be sent to handler.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <typeparam name="TId">Type of aggregate id.</typeparam>
    /// <typeparam name="TPayload">Type of command payload.</typeparam>
    /// <returns>Result of command execution.</returns>
    /// <exception cref="InvalidOperationException">Handler not registered.</exception>
    /// <remarks>
    /// Command source is the place where command was sent.
    /// 
    /// It's important to have a good performance use a specific type of TPayload instead of general ICommand.
    /// When ICommand passed as TPayload bus has to use reflection to find the proper handler and create command envelope.
    /// When a specific type passed as TPayload reflection not needed.
    /// </remarks>
    public Task<ICommandExecutionResult<TId>> Send<TId, TPayload>(ICommandEnvelope<TId> commandEnvelope, CancellationToken cancellationToken = default)
        where TPayload : ICommand
    {
        ByDelegateActivation activator = GetActivator<TId, TPayload>((TPayload)commandEnvelope.Payload);

        ICommandHandler instance = (ICommandHandler)ActivatorUtilities.GetServiceOrCreateInstance(_provider, activator.HandlerType);
        instance.Engine = _provider.GetRequiredService<IEventSourcingEngine>();
        
        Task? result;
        if (activator.UseCancellation)
        {
            result = activator.ExecuteCommandWithCancellationDelegate!.Invoke(instance, commandEnvelope, cancellationToken);
        }
        else
        {
            result = activator.ExecuteCommandDelegate!.Invoke(instance, commandEnvelope);
        }

        Task<ICommandExecutionResult<TId>>? task = result as Task<ICommandExecutionResult<TId>>;
        if (task == null)
        {
            Thrown.InvalidOperationException("Command handler method must return Task<ICommandExecutionResult<TId>>");
        }

        return task;
    }

    private ByDelegateActivation GetActivator<TId, TPayload>(TPayload payload) where TPayload : ICommand
    {
        Type handlerType;
        if (typeof(TPayload) != typeof(ICommand))
        {
            handlerType = typeof(ICommandEnvelope<TId, TPayload>);
        }
        else
        {
            handlerType = typeof(ICommandEnvelope<,>).MakeGenericType(typeof(TId), payload.GetType());
        }

        if(!_handlers.TryGetValue(handlerType, out ByDelegateActivation activator))
        {
            Thrown.InvalidOperationException($"Handler for type {handlerType} not registered");
        }

        return activator;
    }

    private ByDelegateActivation CreateDelegateActivation(CommandHandlerActivation activation)
    {
        ParameterInfo[] paramInfos = activation.Method.GetParameters();
        ExecuteCommandDelegate? executeCommandDelegate = null;
        ExecuteCommandWithCancellationDelegate? executeCommandWithCancellationDelegate = null;
        
        ParameterExpression targetParameter = Expression.Parameter(typeof(object), "handler");
        ParameterExpression envelopeParameter = Expression.Parameter(typeof(ICommandEnvelope), "envelope");
        
        if (activation.UseCancellation)
        {
            if (paramInfos.Length != 2)
            {
                Thrown.InvalidOperationException("Command consumer with cancellation must accept 2 parameters");
            }
            ParameterExpression cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "token");
            
            MethodCallExpression methodCallExpression = Expression.Call(
                Expression.Convert(targetParameter, activation.Type),
                activation.Method,
                Expression.Convert(envelopeParameter, paramInfos[0].ParameterType),
                cancellationTokenParameter
            );
            Expression<ExecuteCommandWithCancellationDelegate> consumeExpr = Expression.Lambda<ExecuteCommandWithCancellationDelegate>(
                methodCallExpression,
                targetParameter,
                envelopeParameter,
                cancellationTokenParameter
            );
            executeCommandWithCancellationDelegate = consumeExpr.Compile();
        }
        else
        {
            if (paramInfos.Length != 1)
            {
                Thrown.InvalidOperationException("Command consumer without cancellation must accept only 1 parameter");    
            }
            
            MethodCallExpression methodCallExpression = Expression.Call(
                Expression.Convert(targetParameter, activation.Type),
                activation.Method,
                Expression.Convert(envelopeParameter, paramInfos[0].ParameterType)
            );
            Expression<ExecuteCommandDelegate> consumeExpr = Expression.Lambda<ExecuteCommandDelegate>(
                methodCallExpression,
                targetParameter,
                envelopeParameter
            );
            executeCommandDelegate = consumeExpr.Compile();
        }

        return new ByDelegateActivation(
            activation.Type,
            executeCommandDelegate,
            executeCommandWithCancellationDelegate,
            executeCommandWithCancellationDelegate != null);
    }

    private delegate Task ExecuteCommandDelegate(object handler, ICommandEnvelope envelope);

    private delegate Task ExecuteCommandWithCancellationDelegate(object handler, ICommandEnvelope envelope, CancellationToken token);

    private sealed record ByDelegateActivation(Type HandlerType,
                                        ExecuteCommandDelegate? ExecuteCommandDelegate,
                                        ExecuteCommandWithCancellationDelegate? ExecuteCommandWithCancellationDelegate, bool UseCancellation);
}