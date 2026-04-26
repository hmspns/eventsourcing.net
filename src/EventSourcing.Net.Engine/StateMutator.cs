using System;
using System.Collections.Generic;
using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Net.Engine;

using Abstractions;
using Collections;

/// <inheritdoc />
public abstract class StateMutator<TState> : IDisposable, IStateMutator<TState> where TState : class
{
    private static readonly HybridDictionary<Type, InternalMutateStateDelegate<TState>> _staticHandlers = new();
        
    private HybridDictionary<Type, InternalMutateStateDelegate<TState>>? _instanceHandlers;
    
    /// <summary>
    /// Register event handler.
    /// </summary>
    /// <param name="handler">Handler.</param>
    /// <typeparam name="TId">Type of id.</typeparam>
    /// <typeparam name="TPayload">Type of payload.</typeparam>
    /// <remarks>This is the same operation as "Register", but register handler in a static dictionary. It's more efficient than instance registration.</remarks>
    protected static void RegisterStatic<TId, TPayload>(MutateStateDelegate<TId, TPayload, TState> handler) where TPayload : IEvent
    {
        // use non-generic version of IEventEnvelop<TId, TPayload> to allow call from non-generic context.
        TState Wrapper(IEventEnvelope @event, TState state)
        {
            return handler((IEventEnvelope<TId, TPayload>)@event, state);
        }
        
        _staticHandlers[typeof(TPayload)] = Wrapper;
    }
        
    /// <summary>
    /// Register event handler.
    /// </summary>
    /// <param name="handler">Handler.</param>
    /// <typeparam name="TId">Type of id.</typeparam>
    /// <typeparam name="TPayload">Type of payload.</typeparam>
    protected void Register<TId, TPayload>(MutateStateDelegate<TId, TPayload, TState> handler) where TPayload : IEvent
    {
        if (_instanceHandlers == null)
        {
            _instanceHandlers = new HybridDictionary<Type, InternalMutateStateDelegate<TState>>();
        }
        
        // use non-generic version of IEventEnvelop<TId, TPayload> to allow call from non-generic context.
        TState Wrapper(IEventEnvelope @event, TState state)
        {
            return handler((IEventEnvelope<TId, TPayload>)@event, state);
        }
        
        _instanceHandlers[typeof(TPayload)] = Wrapper;
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
        Type type = eventEnvelope.Payload.GetType();
        if (_staticHandlers.Count > 0 && _staticHandlers.TryGetValue(type, out InternalMutateStateDelegate<TState>? handler))
        {
            Current = handler(eventEnvelope, Current);
            return Current;
        }
        
        if (_instanceHandlers != null && _instanceHandlers.TryGetValue(type, out handler))
        {
            Current = handler(eventEnvelope, Current);
            return Current;
        }
        
        Exceptions.Thrown.InvalidOperationException($"Couldn't find handler for type {type.FullName}");
        return default; // this line never will be called
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

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <param name="disposing">True for explicit disposing, false for disposing from finalizer.</param>
    /// <remarks>Don't forget to call <b>base.Dispose(disposing)</b> in case of override.</remarks>
    protected virtual void Dispose(bool disposing)
    {
        if(Current is IDisposable stateDisposer)
        {
            stateDisposer.Dispose();
        }
    }

    /// <summary></summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

internal delegate TState InternalMutateStateDelegate<TState>(IEventEnvelope e, TState state);