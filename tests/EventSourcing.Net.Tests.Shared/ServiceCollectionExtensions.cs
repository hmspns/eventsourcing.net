using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net.Tests.Shared;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Check whether service collection contains specific service.
    /// </summary>
    /// <param name="services">Services.</param>
    /// <typeparam name="TService">Type of service.</typeparam>
    /// <returns>Does service collection contains specific service.</returns>
    public static bool IsRegistered<TService>(this IServiceCollection services)
    {
        return services.Any(x => x.ServiceType == typeof(TService));
    }

    /// <summary>
    /// Check whether service collection don't contains specific service.
    /// </summary>
    /// <param name="services">Services.</param>
    /// <typeparam name="TService">Type of service.</typeparam>
    /// <returns>Does service collection not contains specific service.</returns>
    public static bool IsNotRegistered<TService>(this IServiceCollection services)
    {
        return !IsRegistered<TService>(services);
    }
}