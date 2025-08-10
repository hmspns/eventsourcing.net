namespace EventSourcing.Net.Telemetry.Services;

#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

using System.Diagnostics;
using Abstractions.Contracts;
using Abstractions.Identities;
using Contracts;
using Engine.Extensions;
using Internal;
using Microsoft.Extensions.DependencyInjection;

/// <inheritdoc />
public sealed class InMemoryEventCallByExpressionPublisherWithTelemetryResolver : IResolveEventPublisher
{
    private readonly IEventPublisher _publisher;

    internal InMemoryEventCallByExpressionPublisherWithTelemetryResolver(IServiceProvider provider,
                                                                         IPublicationTelemetryService telemetryService,
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
        
        _publisher = new InMemoryCallByExpressionEventPublisherWithTelemetry(provider, telemetryService, localHandlers);
    }

    public IEventPublisher Get(TenantId tenantId)
    {
        return _publisher;
    }
}

/// <inheritdoc />
public sealed class InMemoryCallByExpressionEventPublisherWithTelemetry : IEventPublisher
{
    private readonly IReadOnlyDictionary<Type, SpecificMethodActivator[]> _handlers;
    private readonly IServiceProvider _provider;
    private readonly IPublicationTelemetryService _telemetryService;

    internal InMemoryCallByExpressionEventPublisherWithTelemetry(IServiceProvider provider,
                                                                 IPublicationTelemetryService telemetryService,
                                                                 IReadOnlyDictionary<Type, SpecificMethodActivator[]> handlers)
    {
        _telemetryService = telemetryService;
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
                    Stopwatch st = Stopwatch.StartNew();
                    object instance = ActivatorUtilities.GetServiceOrCreateInstance(scope.ServiceProvider, activator.HandlerType);
                    
                    Task result = activator.Consumer(instance, envelope);
                    if (result != null)
                    {
                        await result.ConfigureAwait(false);
                    }
                    
                    TimeSpan elapsed = st.Elapsed;
                    _telemetryService.AddTelemetry(envelope.GetType(), envelope.Payload.GetType(), activator.HandlerType, elapsed);;
                }
            }
        }
    }
}