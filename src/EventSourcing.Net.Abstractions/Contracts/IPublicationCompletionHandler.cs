namespace EventSourcing.Net.Abstractions.Contracts;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Handler to notify that events publication is done.
/// </summary>
public interface IPublicationCompletionHandler
{
    /// <summary>
    /// Called when publication is done.
    /// </summary>
    /// <returns>Task/</returns>
    Task PublicationDone();
}

/// <summary>
/// Handler to notify that events publication is started.
/// </summary>
public interface IPublicationStartedHandler
{
    /// <summary>
    /// Called when publication is started.
    /// </summary>
    /// <param name="matchedEvents">Events related to consumer.</param>
    /// <returns></returns>
    Task PublicationStarting(BatchEvents matchedEvents);
}

/// <summary>
/// Represents a batch of events for publication.
/// </summary>
public readonly record struct BatchEvents
{
    private readonly List<IEventEnvelope> _events = new List<IEventEnvelope>(16);

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchEvents"/> struct.
    /// </summary>
    public BatchEvents()
    {
    }

    /// <summary>
    /// Adds an event envelope to the batch.
    /// </summary>
    /// <param name="eventEnvelope">The event envelope to add.</param>
    internal void Add(IEventEnvelope eventEnvelope)
    {
        _events.Add(eventEnvelope);
    }
    
    /// <summary>
    /// Gets all events in the batch.
    /// </summary>
    public IEnumerable<IEventEnvelope> Events => _events;
    
    /// <summary>
    /// Gets events with a specific identifier type.
    /// </summary>
    /// <typeparam name="TId">The type of the event identifier.</typeparam>
    /// <returns>A collection of typed event envelopes.</returns>
    public IEnumerable<IEventEnvelope<TId>> GetTypedEvents<TId>()
    {
        return Events.OfType<IEventEnvelope<TId>>();
    }
}