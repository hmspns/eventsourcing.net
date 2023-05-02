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
        ISnapshot snapshot = await snapshotStore.LoadSnapshot(streamId);
        EventsStream events = await eventStore.LoadEventsStream<TId>(streamId, (StreamPosition)snapshot.Version, StreamPosition.End);
            
        TAggregate aggregate = _activator(commandEnvelope.AggregateId);

        aggregate.LoadSnapshot(snapshot);
        aggregate.LoadEvents(events);

        ICommandExecutionResult<TId> aggregationResult = handler(aggregate);

        if (aggregate.Uncommitted.Any())
        {
            try
            {
                if (cancellationToken.CancellationWasRequested(commandEnvelope, out ICommandExecutionResult<TId> cancelledResult))
                {
                    return cancelledResult;
                }
                    
                IEventPublisher eventPublisher = _engine.PublisherResolver.Get(commandEnvelope.TenantId);
                IAppendEventsResult result = await eventStore.AppendToStream<TId>(commandEnvelope, aggregate.StreamName, aggregate.Version, aggregate.Uncommitted);
                await eventPublisher.Publish(commandEnvelope, aggregate.Uncommitted);
                await snapshotStore.SaveSnapshot(aggregate.StreamName, aggregate.GetSnapshot(result));
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
}