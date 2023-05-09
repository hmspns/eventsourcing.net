using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net;

public static class EventSourcingServiceProviderExtensions
{
    /// <summary>
    /// Start event sourcing engine.
    /// </summary>
    /// <param name="provider">Service provider.</param>
    /// <param name="initializeStorageForDefaultTenant">Should storage be initialized for default tenant (TenantId.Empty).</param>
    /// <remarks>For multitenancy environment you should manually initialize storage per tenant.</remarks>
    public static Task StartEventSourcingEngine(this IServiceProvider provider, bool initializeStorageForDefaultTenant = true)
    {
        EventSourcingEngineStarter starter = provider.GetRequiredService<EventSourcingEngineStarter>();
        return starter.Start(initializeStorageForDefaultTenant);
    }
}