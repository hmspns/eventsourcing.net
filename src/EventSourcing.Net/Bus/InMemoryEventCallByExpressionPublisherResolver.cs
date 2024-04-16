namespace EventSourcing.Net;

#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

using System.Linq.Expressions;
using System.Reflection;
using Abstractions.Contracts;
using Abstractions.Identities;
using Engine.Exceptions;
using Internal;
using Microsoft.Extensions.DependencyInjection;

/// <inheritdoc />
public sealed class InMemoryEventCallByExpressionPublisherResolver : IResolveEventPublisher
{
    private readonly InMemoryCallByExpressionEventPublisher _publisher;

    internal InMemoryEventCallByExpressionPublisherResolver(IServiceProvider provider,
                                                        IReadOnlyDictionary<Type, EventConsumerActivation[]> handlers)
    {
        IReadOnlyDictionary<Type, SpecificMethodActivator[]> localHandlers = handlers
            .ToDictionary(
                x => x.Key,
                x => x.Value.Select(GetActivator).ToArray()
                );
#if NET8_0_OR_GREATER
        localHandlers = localHandlers.ToFrozenDictionary();
#endif

        _publisher = new InMemoryCallByExpressionEventPublisher(provider, localHandlers);
    }

    public IEventPublisher Get(TenantId tenantId)
    {
        return _publisher;
    }

    private SpecificMethodActivator GetActivator(EventConsumerActivation activation)
    {
        ParameterInfo[] paramInfos = activation.Method.GetParameters();

        if (paramInfos.Length != 1)
        {
            Thrown.InvalidOperationException("Event consumer must accept only 1 parameter");    
        }
        
        ParameterExpression targetParameter = Expression.Parameter(typeof(object), "handler");
        ParameterExpression envelopeParameter = Expression.Parameter(typeof(IEventEnvelope), "envelope");

        MethodCallExpression methodCallExpression = Expression.Call(
            Expression.Convert(targetParameter, activation.Type),
            activation.Method,
            Expression.Convert(envelopeParameter, paramInfos[0].ParameterType)
        );
        Expression<EventConsumerDelegate> consumeExpr = Expression.Lambda<EventConsumerDelegate>(
            methodCallExpression,
            targetParameter,
            envelopeParameter
        );

        EventConsumerDelegate consumer = consumeExpr.Compile();
        return new SpecificMethodActivator(activation.Type, consumer);
    }
}

/// <inheritdoc />
public sealed class InMemoryCallByExpressionEventPublisher : IEventPublisher
{
    private readonly IReadOnlyDictionary<Type, SpecificMethodActivator[]> _handlers;
    private readonly IServiceProvider _provider;

    internal InMemoryCallByExpressionEventPublisher(IServiceProvider provider,
                                                IReadOnlyDictionary<Type, SpecificMethodActivator[]> handlers)
    {
        _provider = provider;
        _handlers = handlers;
    }

    public async Task Publish(ICommandEnvelope? commandEnvelope, IReadOnlyList<IEventEnvelope> events)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        foreach (IEventEnvelope envelope in events)
        {
            Type envelopeType = envelope.GetEnvelopeTypedInterface();
            
            if (_handlers.TryGetValue(envelopeType, out SpecificMethodActivator[]? activators))
            {
                foreach (SpecificMethodActivator activator in activators)
                {
                    object instance = ActivatorUtilities.GetServiceOrCreateInstance(scope.ServiceProvider, activator.HandlerType);
                    Task result = activator.Consumer.Invoke(instance, envelope);
                    if (result != null)
                    {
                        await result.ConfigureAwait(false);
                    }
                }
            }
        }
    }
}

internal delegate Task EventConsumerDelegate(object handler, IEventEnvelope envelope);

internal sealed record SpecificMethodActivator(Type HandlerType, EventConsumerDelegate Consumer);