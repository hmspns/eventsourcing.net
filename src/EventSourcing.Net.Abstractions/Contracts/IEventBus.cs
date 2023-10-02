using System.Threading.Tasks;

namespace EventSourcing.Net.Abstractions.Contracts;

/// <summary>
/// Bus to send events to consumers.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Send the event to the bus.
    /// </summary>
    /// <param name="eventEnvelope">Event envelope.</param>
    Task Send(IEventEnvelope eventEnvelope);
}