using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net;

public static class EventSourcingServicesExtensions
{
    /// <summary>
    /// Add DI for event sourcing.
    /// </summary>
    /// <param name="services">Services.</param>
    /// <returns>Builder to configure services.</returns>
    public static IServiceCollection AddEventSourcing(this IServiceCollection services, Action<EventSourcingOptions> handler)
    {
        var builder = new EventSourcingOptions(services);

        return services;
    }
}