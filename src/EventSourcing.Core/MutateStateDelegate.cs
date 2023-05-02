using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Core;

/// <summary>
/// Delegate to register handler to mutate state.
/// </summary>
/// <typeparam name="TId">Type of aggregate id.</typeparam>
/// <typeparam name="TPayload">Type of event payload.</typeparam>
/// <typeparam name="TState">Type of state.</typeparam>
public delegate TState MutateStateDelegate<in TId, in TPayload, TState>(IEventEnvelope<TId, TPayload> e, TState state)
    where TPayload : IEvent;