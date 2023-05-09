using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net;

/// <summary>
/// Options to configure storage.
/// </summary>
public sealed class EventSourcingStorageOptions : EventSourcingConfigurationOptions
{
    /// <summary>
    /// Initialize new object.
    /// </summary>
    /// <param name="services">Services to provide DI registration.</param>
    internal EventSourcingStorageOptions(IServiceCollection services) : base(services)
    {
    }
}