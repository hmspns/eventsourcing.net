namespace EventSourcing.Net.Telemetry.Services;

#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif
using System.Diagnostics;
using Abstractions.Contracts;
using Abstractions.Identities;
using Engine.Collections;
using Engine.Extensions;
using Engine.Pooled.Collections;
using Internal;
using Microsoft.Extensions.DependencyInjection;

/// <inheritdoc />
public sealed class InMemoryEventPublisherWithPreloadResolver : IResolveEventPublisher
{
    private readonly IEventPublisher _publisher;

    internal InMemoryEventPublisherWithPreloadResolver(IServiceProvider provider,
                                                       IReadOnlyDictionary<Type, EventConsumerActivation[]> handlers)
    {
        IReadOnlyDictionary<Type, SpecificMethodActivator[]> localHandlers = handlers
            .ToDictionary(
                x => x.Key,
                x => x.Value.Select(InMemoryEventCallByExpressionPublisherResolver.GetActivator).ToArray()
            );
#if NET8_0_OR_GREATER
        localHandlers = localHandlers.ToFrozenDictionary();
#endif
        
        _publisher = new InMemoryEventPublisherWithPreload(provider, localHandlers);
    }

    public IEventPublisher Get(TenantId tenantId)
    {
        return _publisher;
    }
}


public class InMemoryEventPublisherWithPreload : IEventPublisher
{
    private readonly IReadOnlyDictionary<Type, SpecificMethodActivator[]> _handlers;
    private readonly IServiceProvider _provider;

    internal InMemoryEventPublisherWithPreload(IServiceProvider provider,
                                       IReadOnlyDictionary<Type, SpecificMethodActivator[]> handlers)
    {
        _provider = provider;
        _handlers = handlers;
    }


    public async Task Publish(ICommandEnvelope? commandEnvelope, IReadOnlyList<IEventEnvelope> events)
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        HybridSet<IPublicationCompletionHandler>? publicationCompletionHandlers = null;

        using PooledList<Func<Task>> consumers = new PooledList<Func<Task>>(events.Count);
        Dictionary<IPublicationStartedHandler, BatchEvents> mapping = new Dictionary<IPublicationStartedHandler, BatchEvents>(events.Count);
        foreach (IEventEnvelope envelope in events)
        {
            Type envelopeType = envelope.GetEnvelopeTypedInterface();
            
            if (_handlers.TryGetValue(envelopeType, out SpecificMethodActivator[]? activators))
            {
                foreach (SpecificMethodActivator activator in activators)
                {
                    object instance = ActivatorUtilities.GetServiceOrCreateInstance(scope.ServiceProvider, activator.HandlerType);

                    if (instance is IPublicationStartedHandler publicationStartingHandler)
                    {
                        if(!mapping.TryGetValue(publicationStartingHandler, out BatchEvents batchEvents))
                        {
                            batchEvents = new BatchEvents();
                            mapping.Add(publicationStartingHandler, batchEvents);
                        }
                    
                        batchEvents.Add(envelope);
                    }
                    
                    consumers.Add(() =>
                    {
                        Task result = activator.Consumer(instance, envelope);
                        if (result != null)
                        {
                            return result;
                        }

                        return Task.CompletedTask;
                    });

                    
                    if (instance is IPublicationCompletionHandler publicationCompletionHandler)
                    {
                        publicationCompletionHandlers ??= new HybridSet<IPublicationCompletionHandler>();
                        publicationCompletionHandlers.Add(publicationCompletionHandler);
                    }
                }
            }
        }

        foreach (KeyValuePair<IPublicationStartedHandler, BatchEvents> pair in mapping)
        {
            await pair.Key.PublicationStarting(pair.Value).ConfigureAwait(false);
        }

        foreach (Func<Task> consumer in consumers)
        {
            await consumer().ConfigureAwait(false);
        }
        
        if(publicationCompletionHandlers != null)
        {
            foreach (IPublicationCompletionHandler publicationCompletionHandler in publicationCompletionHandlers)
            {
                await publicationCompletionHandler.PublicationDone().ConfigureAwait(false);
            }
        }
    }
}