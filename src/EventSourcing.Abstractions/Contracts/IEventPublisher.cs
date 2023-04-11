using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Abstractions.Contracts;

/// <summary>
/// Factory to resolve tenant specific event publisher.
/// </summary>
public interface IResolveEventPublisher
{
    /// <summary>
    /// Return tenant specific event publisher.
    /// </summary>
    /// <param name="tenantId">Tenant id.</param>
    /// <returns>Tenant specific event publisher.</returns>
    IEventPublisher Get(TenantId tenantId);
}
    
/// <summary>
/// Event publisher.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publish event.
    /// </summary>
    /// <param name="commandEnvelope">Command data.</param>
    /// <param name="events">Events to be published.</param>
    Task Publish(ICommandEnvelope commandEnvelope, IReadOnlyList<IEventEnvelope> events);
}