using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Net.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net;

public sealed class InMemoryEventPublisherResolver : IResolveEventPublisher
{
    private readonly IServiceProvider _provider;
    private readonly IReadOnlyDictionary<Type, EventConsumerActivation[]> _handlers;

    internal InMemoryEventPublisherResolver(IServiceProvider provider, IReadOnlyDictionary<Type, EventConsumerActivation[]> handlers)
    {
        _provider = provider;
        _handlers = handlers;
    }
    
    public IEventPublisher Get(TenantId tenantId)
    {
        return new InMemoryEventPublisher(_provider, _handlers);
    }
}

public sealed class InMemoryEventPublisher : IEventPublisher
{
    private readonly IReadOnlyDictionary<Type, EventConsumerActivation[]> _handlers;
    private readonly IServiceProvider _provider;

    internal InMemoryEventPublisher(IServiceProvider provider, IReadOnlyDictionary<Type, EventConsumerActivation[]> handlers)
    {
        _provider = provider;
        _handlers = handlers;
    }
    
    public async Task Publish(ICommandEnvelope commandEnvelope, IReadOnlyList<IEventEnvelope> events)
    {
        var envelopeType = commandEnvelope.GetType();
        if (_handlers.TryGetValue(envelopeType, out var activators))
        {
            foreach (IEventEnvelope envelope in events)
            {
                foreach (var activator in activators)
                {
                    object instance = ActivatorUtilities.GetServiceOrCreateInstance(_provider, activator.Type);
                    Task result = (Task)activator.Method.Invoke(instance, new[] { envelope });
                    await result;
                }
            }
        }
    }
}