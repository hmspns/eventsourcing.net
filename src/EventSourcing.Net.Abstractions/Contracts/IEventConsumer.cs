namespace EventSourcing.Net.Abstractions.Contracts;

using System.Threading.Tasks;

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

/// <summary>
/// Implementation of event consumption method.
/// </summary>
/// <typeparam name="TEventType">Type of event/</typeparam>
public interface ISagaConsumer<TId, in TEventType> where TEventType : IEvent
{
    /// <summary>
    /// Consume specific event by saga.
    /// </summary>
    /// <param name="envelope">Envelope with information about event.</param>
    /// <returns>Awaitable task.</returns>
    Task Consume(IEventEnvelope<TId, TEventType> envelope);
}