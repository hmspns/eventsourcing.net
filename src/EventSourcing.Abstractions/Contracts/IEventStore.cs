using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Abstractions.Types;

namespace EventSourcing.Abstractions.Contracts;

/// <summary>
/// Factory to resolve tenant specific event store.
/// </summary>
public interface IResolveEventStore
{
    /// <summary>
    /// Return tenant specific event store.
    /// </summary>
    /// <param name="tenantId">Tenant id.</param>
    /// <returns>Tenant specific event store.</returns>
    IEventStore Get(TenantId tenantId);
}
    
/// <summary>
/// Event store.
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Return events stream.
    /// </summary>
    /// <param name="streamName">Name of the stream.</param>
    /// <param name="from">Start position.</param>
    /// <param name="to">End position.</param>
    /// <returns>Events stream.</returns>
    Task<EventsStream> LoadEventsStream<TId>(StreamId streamName, StreamPosition from, StreamPosition to);

    /// <summary>
    /// Add events to the stream.
    /// </summary>
    /// <param name="commandEnvelope">Command data.</param>
    /// <param name="streamName">Name of the stream.</param>
    /// <param name="aggregateVersion">Aggregate version.</param>
    /// <param name="events">Events that should be added.</param>
    /// <returns>Result of the operation.</returns>
    Task<IAppendEventsResult> AppendToStream<TId>(ICommandEnvelope<TId> commandEnvelope, StreamId streamName, AggregateVersion aggregateVersion, IReadOnlyList<IEventEnvelope> events);
}