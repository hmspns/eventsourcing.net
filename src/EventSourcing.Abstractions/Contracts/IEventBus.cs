using System.Threading.Tasks;

namespace EventSourcing.Abstractions.Contracts;

/// <summary>
/// Event bus.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Send the event to the bus.
    /// </summary>
    /// <param name="eventEnvelope">Event envelope.</param>
    Task Send(IEventEnvelope eventEnvelope);
}