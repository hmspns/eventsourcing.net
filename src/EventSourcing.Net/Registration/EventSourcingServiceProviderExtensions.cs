using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net;

public static class EventSourcingServiceProviderExtensions
{
    /// <summary>
    /// Start event sourcing engine.
    /// </summary>
    /// <param name="provider">Service provider.</param>
    public static Task StartEventSourcingEngine(this IServiceProvider provider)
    {
        EventSourcingEngineStarter starter = provider.GetRequiredService<EventSourcingEngineStarter>();
        return starter.Start();
    }
}