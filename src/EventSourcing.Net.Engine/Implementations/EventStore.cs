using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Abstractions.Types;
using EventSourcing.Net.Engine.Extensions;

namespace EventSourcing.Net.Engine.Implementations;

using Pooled.Collections;

/// <inheritdoc />
public sealed class EventStoreResolver : IResolveEventStore
{
    private readonly IResolveAppender _appenderResolver;
    private readonly IAggregateIdParsingProvider _aggregateIdParsingProvider;

    public EventStoreResolver(IResolveAppender appenderResolver, IAggregateIdParsingProvider aggregateIdParsingProvider)
    {
        _aggregateIdParsingProvider = aggregateIdParsingProvider;
        _appenderResolver = appenderResolver;
    }
        
    public IEventStore Get(TenantId tenantId)
    {
        return new EventStore(_appenderResolver.Get(tenantId), _aggregateIdParsingProvider);
    }
}

/// <inheritdoc />
public sealed class EventStore : IEventStore
{
    private readonly IAppendOnly _appender;
    private readonly IAggregateIdParsingProvider _aggregateIdParsingProvider;

    public EventStore(IAppendOnly appender, IAggregateIdParsingProvider aggregateIdParsingProvider)
    {
        _aggregateIdParsingProvider = aggregateIdParsingProvider;
        _appender = appender;
    }
        
    public async Task<EventsStream> LoadEventsStream<TId>(StreamId streamName, StreamPosition from, StreamPosition to)
    {
        using IEventsData dbEvents = await _appender.ReadSpecificStream(streamName, from, to).ConfigureAwait(false);

        PooledList<IEventEnvelope> events = new PooledList<IEventEnvelope>(dbEvents.Events.Count);
        if (dbEvents.Events.Count > 0)
        {
            Func<string, object> parser = _aggregateIdParsingProvider.GetParser(typeof(TId));
            foreach (EventPackage eventPackage in dbEvents.Events)
            {
                IEventEnvelope eventEnvelope = eventPackage.ToEventEnvelope<TId>(parser);
                events.Add(eventEnvelope);
            }
        }

        AggregateVersion version = AggregateVersion.NotCreated;
        if (dbEvents.Events.Any())
        {
            version = dbEvents.Events.Max(x => x.StreamPosition);
        }
            
        return new EventsStream()
        {
            Events = events,
            From = from,
            To = to,
            Version = version,
            HasEvents = dbEvents.Events.Any(),
            StreamName = streamName,
            IsEndOfStream = version == dbEvents.StreamEndPosition
        };
    }

    public async Task<IAppendEventsResult> AppendToStream<TId>(ICommandEnvelope<TId> commandEnvelope, StreamId streamName, AggregateVersion aggregateVersion, IReadOnlyList<IEventEnvelope> events)
    {
        AppendCommandPackage<TId> commandPackage = new ()
        {
            Payload = commandEnvelope.Payload,
            Source = commandEnvelope.Source,
            Timestamp = commandEnvelope.Timestamp,
            AggregateId = commandEnvelope.AggregateId,
            CommandId = commandEnvelope.CommandId,
            PrincipalId = commandEnvelope.PrincipalId,
            SequenceId = commandEnvelope.SequenceId,
            ParentCommandId = commandEnvelope.ParentCommandId,
            TenantId = commandEnvelope.TenantId
        };

        AppendEventPackage[] eventPackages = new AppendEventPackage[events.Count];
        for (int i = 0; i < events.Count; i++)
        {
            IEventEnvelope envelope = events[i];
            eventPackages[i] = new AppendEventPackage()
            {
                Payload = envelope.Payload,
                Timestamp = envelope.Timestamp,
                EventId = envelope.EventId,
                StreamName = streamName
            };
        }
        
        AppendDataPackage<TId> package = new AppendDataPackage<TId>(commandPackage, eventPackages);

        IAppendEventsResult result = await _appender.Append<TId>(streamName, package, aggregateVersion).ConfigureAwait(false);
        return result;
    }
}