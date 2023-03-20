using System;
using System.Threading.Tasks;
using EventSourcing.Abstractions;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Core.Contracts;

namespace EventSourcing.Core.Implementations;

/// <summary>
/// Load state for aggregate.
/// </summary>
/// <typeparam name="TId">Aggregate id type.</typeparam>
/// <typeparam name="TAggregate">Aggregate type.</typeparam>
/// <typeparam name="TState">State type.</typeparam>
public sealed class AggregateStateLoader<TId, TAggregate, TState> : IAggregateStateLoader<TId, TState>
    where TAggregate : IAggregate<TId>
    where TState : class
{
    private readonly IEventSourcingEngine _engine;
    private readonly Func<IStateMutator<TState>> _activator;

    /// <summary>
    /// Initialize new object.
    /// </summary>
    /// <param name="engine">Event sourcing engine.</param>
    /// <exception cref="InvalidOperationException">Aggregate type not inherited from Aggregate<TId, TState, TStateMutator></exception>
    public AggregateStateLoader(IEventSourcingEngine engine)
    {
        _engine = engine;
        Type baseType = typeof(Aggregate<,,>);
        Type aggregateType = typeof(TAggregate);

        if (aggregateType.BaseType?.GetGenericTypeDefinition() != baseType)
        {
            throw new InvalidOperationException("Aggregate must inherits from Aggregate<TId, TStateMutator, TState>");
        }

        Type[] arguments = aggregateType.BaseType.GetGenericArguments();
        Type stateMutatorType = arguments[2];
        _activator = () => (IStateMutator<TState>)Activator.CreateInstance(stateMutatorType);
    }

    /// <summary>
    /// Initialize new object.
    /// </summary>
    /// <param name="engine">Event sourcing engine.</param>
    /// <param name="activator">Factory method to create state mutator.</param>
    public AggregateStateLoader(IEventSourcingEngine engine, Func<IStateMutator<TState>> activator) : this(engine)
    {
        _activator = activator;
    }

    /// <summary>
    /// Return state for specific aggregate.
    /// </summary>
    /// <param name="tenantId">Tenant id.</param>
    /// <param name="aggregateId">Aggregate id.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <returns>Current state of the aggregate.</returns>
    public async Task<TState> GetState(TenantId tenantId, TId aggregateId)
    {
        StreamId streamId = StreamId.Parse(aggregateId?.ToString());
        
        ISnapshotStore snapshotStore = _engine.SnapshotStoreResolver.Get(tenantId);
        ISnapshot snapshot = await snapshotStore.LoadSnapshot(streamId);

        IEventStore eventStore = _engine.EventStoreResolver.Get(tenantId);
        EventsStream events = await eventStore.LoadEventsStream(streamId, snapshot.Version, StreamPosition.End);

        IStateMutator<TState> stateMutator = _activator();
        object state = snapshot.HasSnapshot ? snapshot.State : stateMutator.DefaultState;
        stateMutator.Transition(state);

        foreach (IEventEnvelope e in events.Events)
        {
            stateMutator.Transition(e);
        }

        return stateMutator.Current;
    }
}