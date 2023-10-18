using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Abstractions.Types;
using EventSourcing.Net.Engine.Exceptions;
using EventSourcing.Net.Engine.Extensions;

namespace EventSourcing.Net.Engine;

using System.Collections.Generic;

/// <summary>
/// Process aggregate flow.
/// </summary>
/// <typeparam name="TId">Type of aggregate id.</typeparam>
/// <typeparam name="TAggregate">Type of aggregate.</typeparam>
internal sealed class AggregateUpdater<TId, TAggregate> where TAggregate : IAggregate
{
    private readonly IEventSourcingEngine _engine;
    private readonly Func<TId, TAggregate> _activator;
        
    internal AggregateUpdater(IEventSourcingEngine engine, Func<TId, TAggregate> activator)
    {
        _activator = activator;
        _engine = engine;
    }
        
    internal async Task<ICommandExecutionResult<TId>> Execute(
        ICommandEnvelope<TId> commandEnvelope,
        Func<TAggregate, ICommandExecutionResult<TId>> handler,
        CancellationToken cancellationToken = default)
    {
        ISnapshotStore snapshotStore = _engine.SnapshotStoreResolver.Get(commandEnvelope.TenantId);
        IEventStore eventStore = _engine.EventStoreResolver.Get(commandEnvelope.TenantId);
            
        StreamId streamId = StreamId.Parse(commandEnvelope.AggregateId.ToString());
        ISnapshot snapshot = await snapshotStore.LoadSnapshot(streamId).ConfigureAwait(false);
        using EventsStream events = await eventStore.LoadEventsStream<TId>(streamId, (StreamPosition)snapshot.Version, StreamPosition.End).ConfigureAwait(false);
            
        TAggregate aggregate = _activator(commandEnvelope.AggregateId);

        try
        {
            aggregate.LoadSnapshot(snapshot);
            aggregate.LoadEvents(events);

            ICommandExecutionResult<TId> aggregationResult = handler(aggregate);

            IReadOnlyList<IEventEnvelope> uncommitted = aggregate.Uncommitted;
            if (uncommitted.Any())
            {
                try
                {
                    if (cancellationToken.CancellationWasRequested(commandEnvelope, out ICommandExecutionResult<TId> cancelledResult))
                    {
                        return cancelledResult;
                    }
                    
                    IAppendEventsResult result = await eventStore
                                                       .AppendToStream<TId>(commandEnvelope, aggregate.StreamName, aggregate.Version, uncommitted)
                                                       .ConfigureAwait(false);
                    IEventPublisher eventPublisher = _engine.PublisherResolver.Get(commandEnvelope.TenantId);
                    
                    await eventPublisher.Publish(commandEnvelope, uncommitted).ConfigureAwait(false);
                    await snapshotStore.SaveSnapshot(aggregate.StreamName, aggregate.GetSnapshot(result)).ConfigureAwait(false);
                }
                catch (AppendOnlyStoreConcurrencyException e)
                {
                    throw new AggregateConcurrencyException<TId>("There is an exception during aggregate execution", e)
                    {
                        AggregateId = commandEnvelope.AggregateId,
                        CommandId = commandEnvelope.CommandId,
                        ExpectedVersion = e.ExpectedStreamVersion,
                        SequenceId = commandEnvelope.SequenceId,
                        Source = commandEnvelope.Source,
                        ActualVersion = e.ActualStreamVersion,
                    };
                }
            }
            
            return aggregationResult;
        }
        finally
        {
            if (aggregate is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}