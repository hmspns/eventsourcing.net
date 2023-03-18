using System;
using System.Collections.Generic;
using EventSourcing.Abstractions;
using EventSourcing.Core.Contracts;

namespace EventSourcing.Core
{
    /// <inheritdoc />
    public abstract class StateMutator<TState> : IStateMutator<TState> where TState : class
    {
        private readonly Dictionary<Type, Func<object, TState, TState>> _handlers = new ();
        
        /// <summary>
        /// Register event handler.
        /// </summary>
        /// <param name="handler">Handler.</param>
        /// <typeparam name="TId">Type of id.</typeparam>
        /// <typeparam name="TPayload">Type of payload.</typeparam>
        protected void Register<TId, TPayload>(Func<EventEnvelope<TId, TPayload>, TState, TState> handler) where TPayload : IEvent
        {
            _handlers[typeof(EventEnvelope<TId, TPayload>)] = (@event, state) => handler((EventEnvelope<TId, TPayload>)@event, state);
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
            if (_handlers.TryGetValue(eventEnvelope.GetType(), out Func<object, TState, TState>? handler))
            {
                Current = handler(eventEnvelope, Current);
                return Current;
            }

            throw new InvalidOperationException($"Couldn't find handler for type {eventEnvelope.GetType().FullName}");
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
}