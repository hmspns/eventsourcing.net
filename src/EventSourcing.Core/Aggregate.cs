using System;
using System.Collections.Generic;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Abstractions.Types;
using EventSourcing.Core.Exceptions;

namespace EventSourcing.Core
{
    /// <summary>
    /// Base class for specific aggregate.
    /// </summary>
    /// <typeparam name="TId">Type of aggregate id.</typeparam>
    /// <typeparam name="TState">Type of state.</typeparam>
    /// <typeparam name="TStateMutator">Type of state mutator.</typeparam>
    public abstract class Aggregate<TId, TState, TStateMutator> : IAggregate<TId>
        where TState : class
        where TStateMutator : IStateMutator<TState>
    {
        private readonly List<IEventEnvelope> _events = new();
        private AggregateVersion _snapshotVersion = AggregateVersion.NotCreated;
        
        /// <summary>
        /// Initialize new object.
        /// </summary>
        /// <param name="aggregateId">Aggregate id.</param>
        /// <param name="stateMutator">Instance of state mutator.</param>
        /// <exception cref="ArgumentNullException">Aggregate id or state mutator is null.</exception>
        /// <exception cref="InvalidOperationException">State mutator or Default state is null.</exception>
        protected Aggregate(TId aggregateId, TStateMutator stateMutator)
        {
            if (aggregateId == null)
            {
                Thrown.ArgumentNullException(nameof(aggregateId));
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (stateMutator == null)
            {
                Thrown.ArgumentNullException(nameof(stateMutator));
            }
            
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (stateMutator.DefaultState == null)
            {
                Thrown.InvalidOperationException($"Aggregate StateMutator must have a default State defined, check {typeof(TStateMutator).ToString()}.DefaultState");
            }
            
            AggregateId = aggregateId;
            Version = AggregateVersion.NotCreated;
            State = stateMutator;
            State.Transition(State.DefaultState);
        }
        
        /// <summary>
        /// Get the current state.
        /// </summary>
        protected TStateMutator State { get; }
        
        /// <summary>
        /// Get aggregate id.
        /// </summary>
        public TId AggregateId { get; }
        
        /// <summary>
        /// Get aggregate version.
        /// </summary>
        public AggregateVersion Version { get; private set; }
        
        /// <summary>
        /// Get produced but not commited events.
        /// </summary>
        public IReadOnlyList<IEventEnvelope> Uncommitted => _events.AsReadOnly();

        /// <summary>
        /// Get stream name.
        /// </summary>
        /// <remarks>This property must be overriden if AggregateId doesn't return correct string representation by ToString method.</remarks>
        public virtual StreamId StreamName => StreamId.Parse(AggregateId!.ToString());

        /// <summary>
        /// Load snapshot into the aggregate.
        /// </summary>
        /// <param name="snapshot">Snapshot</param>
        void IAggregate.LoadSnapshot(ISnapshot snapshot)
        {
            if (snapshot == null)
            {
                Thrown.ArgumentNullException(nameof(snapshot));
            }

            if (snapshot.HasSnapshot)
            {
                State.Transition(snapshot.State);
                Version = snapshot.Version;
                _snapshotVersion = snapshot.Version;
            }
        }

        /// <summary>
        /// Load events into the aggregate.
        /// </summary>
        /// <param name="eventsStream"></param>
        void IAggregate.LoadEvents(EventsStream eventsStream)
        {
            if (eventsStream.From != Version)
            {
                Thrown.InvalidOperationException("Cannot load stream, version is incorrect");
            }

            foreach (IEventEnvelope @event in eventsStream.Events)
            {
                Apply(@event, false);
            }

            if (eventsStream.Version > _snapshotVersion)
            {
                Version = eventsStream.Version;
            }
        }

        /// <summary>
        /// Marked the aggregate as committed.
        /// </summary>
        /// <param name="appendResult">Result of save events process.</param>
        /// <returns>Snapshot of the aggregate.</returns>
        ISnapshot IAggregate.Commit(IAppendEventsResult appendResult)
        {
            _events.Clear();
            return new Snapshot(StreamName, State.Current, appendResult.Version);
        }

        /// <summary>
        /// Apply event.
        /// </summary>
        /// <param name="command">Command data.</param>
        /// <param name="event">Event.</param>
        protected void Apply<TPayload>(ICommandEnvelope<TId> command, TPayload @event) where TPayload : IEvent
        {
            EventEnvelope<TId, TPayload> envelope = new EventEnvelope<TId, TPayload>()
            {
                Payload = @event,
                Timestamp = command.Timestamp,
                AggregateId = AggregateId,
                CommandId = command.CommandId,
                EventId = EventId.New(),
                SequenceId = command.SequenceId,
                Version = Version,
                PrincipalId = command.PrincipalId,
                TenantId = command.TenantId
            };
            Apply(envelope, true);
        }

        /// <summary>
        /// Apply event during loading aggregate.
        /// </summary>
        /// <param name="eventEnvelope">Event data.</param>
        /// <param name="shouldCommitted">Should event be committed.</param>
        private void Apply(IEventEnvelope eventEnvelope, bool shouldCommitted)
        {
            State.Transition(eventEnvelope);
            if (shouldCommitted)
            {
                _events.Add(eventEnvelope);
            }
        }
    }
}