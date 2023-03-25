using System.Threading.Tasks;
using EventSourcing.Abstractions.Contracts;

namespace EventSourcing.Abstractions.ServiceRegistration;

/// <summary>
/// Implementation of event consumption method.
/// </summary>
/// <typeparam name="TEventType">Type of event/</typeparam>
public interface IEventConsumer<TId, in TEventType> where TEventType : IEvent
{
    /// <summary>
    /// Consume specific event.
    /// </summary>
    /// <param name="envelope">Envelope with information about event.</param>
    /// <returns>Awaitable task.</returns>
    Task Consume(IEventEnvelope<TId, TEventType> envelope);
}