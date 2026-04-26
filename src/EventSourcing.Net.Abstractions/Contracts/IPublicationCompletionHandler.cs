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

public interface IPublicationStartedHandler
{
    Task PublicationStarting(BatchEvents matchedEvents);
}

public interface IPublicationStartedHandler<in TId> : IPublicationStartedHandler
{
    Task PublicationStarting(BatchEvents matchedEvents);
}

public readonly record struct BatchEvents
{
    private readonly List<IEventEnvelope> _events = new List<IEventEnvelope>(16);

    public BatchEvents()
    {
    }

    internal void Add(IEventEnvelope eventEnvelope)
    {
        _events.Add(eventEnvelope);
    }
    
    public IEnumerable<IEventEnvelope> Events => _events;
    
    public IEnumerable<IEventEnvelope<TId>> GetTypedEvents<TId>()
    {
        return Events.OfType<IEventEnvelope<TId>>();
    }
}