using System;
using System.Threading.Tasks;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Abstractions.Types;

namespace EventSourcing.Net.Core.Implementations;

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
    /// Initialize new object with default engine.
    /// </summary>
    public AggregateStateLoader() : this(EventSourcingEngine.Instance)
    {
        
    }

    /// <summary>
    /// Initialize new object with default engine.
    /// </summary>
    /// <param name="activator">Factory method to create state mutator.</param>
    public AggregateStateLoader(Func<IStateMutator<TState>> activator) : this(EventSourcingEngine.Instance, activator)
    {
        
    }

    /// <summary>
    /// Return state for specific aggregate.
    /// </summary>
    /// <param name="tenantId">Tenant id.</param>
    /// <param name="aggregateId">Aggregate id.</param>
    /// <param name="useSnapshot">Should snapshot be used to create state.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <returns>Current state of the aggregate.</returns>
    public async Task<TState> GetState(TenantId tenantId, TId aggregateId, bool useSnapshot = true)
    {
        StreamId streamId = StreamId.Parse(aggregateId?.ToString());

        ISnapshot snapshot = Snapshot.Empty(streamId);

        if (useSnapshot)
        {
            ISnapshotStore snapshotStore = _engine.SnapshotStoreResolver.Get(tenantId);
            snapshot = await snapshotStore.LoadSnapshot(streamId).ConfigureAwait(false);
        }

        IEventStore eventStore = _engine.EventStoreResolver.Get(tenantId);
        EventsStream events = await eventStore
            .LoadEventsStream<TId>(streamId, snapshot.Version, StreamPosition.End)
            .ConfigureAwait(false);

        IStateMutator<TState> stateMutator = _activator();
        object state = snapshot.HasSnapshot ? snapshot.State! : stateMutator.DefaultState;
        stateMutator.Transition(state);

        foreach (IEventEnvelope e in events.Events)
        {
            stateMutator.Transition(e);
        }

        return stateMutator.Current;
    }
}