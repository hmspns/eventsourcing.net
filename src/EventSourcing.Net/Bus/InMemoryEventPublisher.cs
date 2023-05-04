using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
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
        await using var scope = _provider.CreateAsyncScope();
        foreach (IEventEnvelope envelope in events)
        {
            Type envelopeType = GetEnvelopeInterfaceType(envelope);
            
            if (_handlers.TryGetValue(envelopeType, out var activators))
            {
                foreach (EventConsumerActivation activator in activators)
                {
                    object instance = ActivatorUtilities.GetServiceOrCreateInstance(scope.ServiceProvider, activator.Type);
                    Task result = (Task)activator.Method.Invoke(instance, new[] { envelope });
                    await result;
                }
            }
        }
    }

    private Type GetEnvelopeInterfaceType(IEventEnvelope envelope)
    {
        Type envelopeType = envelope.GetType();
        Type genericInterfaceType = typeof(IEventEnvelope<,>);

        foreach (Type interfaceType in envelopeType.GetInterfaces())
        {
            if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == genericInterfaceType)
            {
                return interfaceType;
            }
        }

        throw new InvalidOperationException($"Type {envelope.GetType()} should implement interface IEventEnvelope<TId, TPayload>");
    }
}