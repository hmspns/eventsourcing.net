using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Core;

/// <inheritdoc />
public abstract class StateMutator<TState> : IStateMutator<TState> where TState : class
{
    private readonly Dictionary<Type, InternalMutateStateDelegate<TState>> _handlers = new (7);
        
    /// <summary>
    /// Register event handler.
    /// </summary>
    /// <param name="handler">Handler.</param>
    /// <typeparam name="TId">Type of id.</typeparam>
    /// <typeparam name="TPayload">Type of payload.</typeparam>
    protected void Register<TId, TPayload>(MutateStateDelegate<TId, TPayload, TState> handler) where TPayload : IEvent
    {
        // use non-generic version of IEventEnvelop<TId, TPayload> to allow call from non generic context.
        TState Wrapper(IEventEnvelope @event, TState state)
        {
            return handler((IEventEnvelope<TId, TPayload>)@event, state);
        }
        
        _handlers[typeof(TPayload)] = Wrapper;
    }

    /// <summary>
    /// Default state value.
    /// </summary>
    public abstract TState DefaultState { get; }
        
    /// <summary>
    /// Apply event to the state.
    /// </summary>
    /// <param name="eventEnvelope">Event that should be applied.</param>
    /// <returns>State after changes.</returns>
    public TState Transition(IEventEnvelope eventEnvelope)
    {
        if (_handlers.TryGetValue(eventEnvelope.Payload.GetType(), out InternalMutateStateDelegate<TState>? handler))
        {
            Current = handler(eventEnvelope, Current);
            return Current;
        }
            
        throw new InvalidOperationException($"Couldn't find handler for type {eventEnvelope.Payload.GetType().FullName}");
    }

    /// <summary>
    /// Replace state with new state.
    /// </summary>
    /// <param name="state">New state.</param>      
    /// <returns>New state.</returns>
    TState IStateMutator<TState>.Transition(object state)
    {
        if (state is TState t)
        {
            Current = t;
        }

        return Current;
    }

    /// <summary>
    /// Return current state.
    /// </summary>
    public TState Current { get; private set; }
}

internal delegate TState InternalMutateStateDelegate<TState>(IEventEnvelope e, TState state);