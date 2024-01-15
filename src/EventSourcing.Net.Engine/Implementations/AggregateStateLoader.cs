using System;
using System.Threading.Tasks;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Abstractions.Types;

namespace EventSourcing.Net.Engine.Implementations;

using Exceptions;

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
    private readonly Func<IStateMutator<TState>> _stateMutatorActivator;
    private readonly Func<IEventSourcingEngine> _engineHandler;

    private Func<IStateMutator<TState>> CreateActivator()
    {
        Type baseType = typeof(Aggregate<,,>);
        Type aggregateType = typeof(TAggregate);

        if (aggregateType.BaseType?.GetGenericTypeDefinition() != baseType)
        {
            throw new InvalidOperationException("Aggregate must inherits from Aggregate<TId, TStateMutator, TState>");
        }

        Type[] arguments = aggregateType.BaseType.GetGenericArguments();
        Type stateMutatorType = arguments[2];
        return () => (IStateMutator<TState>)Activator.CreateInstance(stateMutatorType);
    }
    
    /// <summary>
    /// Initialize new object.
    /// </summary>
    /// <param name="engine">Event sourcing engine.</param>
    /// <exception cref="InvalidOperationException">Aggregate type not inherited from Aggregate<TId, TState, TStateMutator></exception>
    public AggregateStateLoader(IEventSourcingEngine engine)
    {
        if (engine == null)
        {
            Thrown.ArgumentNullException(nameof(engine), "engine mustn't be null");
        }

        _engineHandler = () => engine;
        _stateMutatorActivator = CreateActivator();
    }

    /// <summary>
    /// Initialize new object.
    /// </summary>
    /// <param name="engine">Event sourcing engine.</param>
    /// <param name="stateMutatorActivator">Factory method to create state mutator.</param>
    public AggregateStateLoader(IEventSourcingEngine engine, Func<IStateMutator<TState>> stateMutatorActivator)
    {
        if (engine == null)
        {
            Thrown.ArgumentNullException(nameof(engine), "engine mustn't be null");
        }
        
        if (stateMutatorActivator == null)
        {
            Thrown.ArgumentNullException(nameof(stateMutatorActivator), "activator mustn't be null");
        }
        
        _engineHandler = () => engine;
        _stateMutatorActivator = stateMutatorActivator;
    }

    /// <summary>
    /// Initialize new object with default engine.
    /// </summary>
    public AggregateStateLoader()
    {
        _engineHandler = () => EventSourcingEngine.Instance;
        _stateMutatorActivator = CreateActivator();
    }

    /// <summary>
    /// Initialize new object with default engine.
    /// </summary>
    /// <param name="stateMutatorActivator">Factory method to create state mutator.</param>
    public AggregateStateLoader(Func<IStateMutator<TState>> stateMutatorActivator)
    {
        if (stateMutatorActivator == null)
        {
            Thrown.ArgumentNullException(nameof(stateMutatorActivator), "activator mustn't be null");
        }
        
        _engineHandler = () => EventSourcingEngine.Instance;
        _stateMutatorActivator = stateMutatorActivator;
    }

    /// <summary>
    /// Return state for the specific aggregate.
    /// </summary>
    /// <param name="aggregateId">Aggregate id.</param>
    /// <param name="useSnapshot">Should snapshot be used to create state.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <returns>Current state of the aggregate.</returns>
    public Task<TState> GetState(TId aggregateId, bool useSnapshot = true)
    {
        return GetState(TenantId.Empty, aggregateId, useSnapshot);
    }

    /// <summary>
    /// Return state for the specific aggregate.
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

        IEventSourcingEngine engine = _engineHandler();
        if (useSnapshot)
        {
            ISnapshotStore snapshotStore = engine.SnapshotStoreResolver.Get(tenantId);
            snapshot = await snapshotStore.LoadSnapshot(streamId).ConfigureAwait(false);
        }

        IEventStore eventStore = engine.EventStoreResolver.Get(tenantId);
        using EventsStream events = await eventStore
            .LoadEventsStream<TId>(streamId, snapshot.Version, StreamPosition.End)
            .ConfigureAwait(false);

        IStateMutator<TState> stateMutator = _stateMutatorActivator();
        object state = snapshot.HasSnapshot ? snapshot.State! : stateMutator.DefaultState;
        stateMutator.Transition(state);

        foreach (IEventEnvelope e in events.Events)
        {
            stateMutator.Transition(e);
        }

        return stateMutator.Current;
    }
}