using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net;

public static class EventSourcingServicesExtensions
{
    /// <summary>
    /// Add DI for event sourcing.
    /// </summary>
    /// <param name="services">Services.</param>
    /// <returns>Builder to configure services.</returns>
    public static IServiceCollection AddEventSourcing(this IServiceCollection services)
    {
        EventSourcingOptions options = new EventSourcingOptions(services);
        options.Build();

        return services;
    }

    /// <summary>
    /// Add DI for event sourcing.
    /// </summary>
    /// <param name="services">Services.</param>
    /// <param name="handler">Action to configure options.</param>
    /// <returns>Builder to configure services.</returns>
    public static IServiceCollection AddEventSourcing(this IServiceCollection services, Action<EventSourcingOptions> handler)
    {
        EventSourcingOptions options = new EventSourcingOptions(services);

        handler(options);
        options.Build();

        return services;
    }
    
    internal static void Remove(this IServiceCollection services, Type interfaceType)
    {
        ServiceDescriptor? descriptor = services.FirstOrDefault(x => x.ServiceType == interfaceType);
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }
    }

    internal static void Remove<TInterface>(this IServiceCollection services) where TInterface : class
    {
        Remove(services, typeof(TInterface));
    }
    
    internal static bool IsRegistered<TInterface>(this IServiceCollection services)
    {
        return services.Any(x => x.ServiceType == typeof(TInterface));
    }

    internal static IServiceCollection IfNotRegistered<T>(
        this IServiceCollection services,
        Action<IServiceCollection> handler)
    {
        if (!services.IsRegistered<T>())
        {
            handler(services);
        }

        return services;
    }
}