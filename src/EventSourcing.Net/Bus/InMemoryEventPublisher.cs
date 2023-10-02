using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Engine.Exceptions;
using EventSourcing.Net.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net;

/// <inheritdoc />
public sealed class InMemoryEventPublisherResolver : IResolveEventPublisher
{
    private readonly IServiceProvider _provider;
    private readonly IReadOnlyDictionary<Type, EventConsumerActivation[]> _handlers;

    internal InMemoryEventPublisherResolver(IServiceProvider provider,
        IReadOnlyDictionary<Type, EventConsumerActivation[]> handlers)
    {
        _provider = provider;
        _handlers = handlers;
    }

    public IEventPublisher Get(TenantId tenantId)
    {
        return new InMemoryEventPublisher(_provider, _handlers);
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

    public async Task Publish(ICommandEnvelope commandEnvelope, IReadOnlyList<IEventEnvelope> events)
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