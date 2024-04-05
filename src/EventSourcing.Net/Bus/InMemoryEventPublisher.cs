using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Engine.Exceptions;
using EventSourcing.Net.Internal;
using Microsoft.Extensions.DependencyInjection;

#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

namespace EventSourcing.Net;

/// <inheritdoc />
public sealed class InMemoryEventPublisherResolver : IResolveEventPublisher
{
    private readonly InMemoryEventPublisher _publisher;

    internal InMemoryEventPublisherResolver(IServiceProvider provider,
        IReadOnlyDictionary<Type, EventConsumerActivation[]> handlers)
    {
        IReadOnlyDictionary<Type, EventConsumerActivation[]> localHandlers;
#if NET8_0_OR_GREATER
        localHandlers = handlers.ToFrozenDictionary();
#else
        localHandlers = handlers;
#endif

        _publisher = new InMemoryEventPublisher(provider, localHandlers);
    }

    public IEventPublisher Get(TenantId tenantId)
    {
        return _publisher;
    }
}

/// <inheritdoc />
public sealed class InMemoryEventPublisher : IEventPublisher
{
    private readonly IReadOnlyDictionary<Type, EventConsumerActivation[]> _handlers;
    private readonly IServiceProvider _provider;

    internal InMemoryEventPublisher(IServiceProvider provider,
        IReadOnlyDictionary<Type, EventConsumerActivation[]> handlers)
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
            
            if (_handlers.TryGetValue(envelopeType, out EventConsumerActivation[]? activators))
            {
                foreach (EventConsumerActivation activator in activators)
                {
                    object instance = ActivatorUtilities.GetServiceOrCreateInstance(scope.ServiceProvider, activator.Type);
                    Task result = (Task)activator.Method.Invoke(instance, new[] { envelope });
                    if (result != null)
                    {
                        await result.ConfigureAwait(false);
                    }
                }
            }
        }
    }
}