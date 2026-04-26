namespace EventSourcing.Net.Telemetry;

using Abstractions.Contracts;
using Contracts;
using Engine.Exceptions;
using Internal;
using Microsoft.Extensions.DependencyInjection;
using Services;

public static class TelemetryExtensions
{
    /// <summary>
    /// Add telemetry.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static EventSourcingOptions UseTelemetry(this EventSourcingOptions options)
    {
        if (options.Bus == null)
        {
            Thrown.InvalidOperationException("Bus is not configured.");
        }
        Dictionary<Type, EventConsumerActivation[]> results = options.Bus.CreateEventConsumers();
        
        options.IfNotRegistered<IPublicationTelemetryService>(services =>
            services.AddSingleton<IPublicationTelemetryService, PublicationTelemetryService>());
        
        options.IfNotRegistered<IResolveEventPublisher>(
            services => services.AddSingleton<IResolveEventPublisher>(x => 
                new InMemoryEventCallByExpressionPublisherWithTelemetryResolver(
                    x,
                    x.GetRequiredService<IPublicationTelemetryService>(),
                    results))
        );
        return options;
    }
    
    /// <summary>
    /// Add telemetry.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static EventSourcingOptions UsePreloadWithTelemetry(this EventSourcingOptions options)
    {
        if (options.Bus == null)
        {
            Thrown.InvalidOperationException("Bus is not configured.");
        }
        Dictionary<Type, EventConsumerActivation[]> results = options.Bus.CreateEventConsumers();
        
        options.IfNotRegistered<IPublicationTelemetryService>(services =>
            services.AddSingleton<IPublicationTelemetryService, PublicationTelemetryService>());
        
        options.IfNotRegistered<IResolveEventPublisher>(
            services => services.AddSingleton<IResolveEventPublisher>(x => 
                new EventPublisherWithPreloadAndTelemetryResolver(
                    x,
                    x.GetRequiredService<IPublicationTelemetryService>(),
                    results))
        );
        return options;
    }
}