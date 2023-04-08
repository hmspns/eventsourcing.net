using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Abstractions.Types;
using EventSourcing.Core.Extensions;

namespace EventSourcing.Core.Implementations;

/// <inheritdoc />
public sealed class EventStoreResolver : IResolveEventStore
{
    private readonly IResolveAppender _appenderResolver;

    public EventStoreResolver(IResolveAppender appenderResolver)
    {
        _appenderResolver = appenderResolver;
    }
        
    public IEventStore Get(TenantId tenantId)
    {
        return new EventStore(_appenderResolver.Get(tenantId));
    }
}

/// <inheritdoc />
public sealed class EventStore : IEventStore
{
    private readonly IAppendOnly _appender;

    public EventStore(IAppendOnly appender)
    {
        _appender = appender;
    }
        
    public async Task<EventsStream> LoadEventsStream<TId>(StreamId streamName, StreamPosition from, StreamPosition to)
    {
        IEventsData dbEvents = await _appender.ReadSpecificStream(streamName, from, to);

        List<IEventEnvelope> events = new List<IEventEnvelope>();
        if (dbEvents.Events.Count > 0)
        {
            Func<string, object> parser = AggregateIdParsingProvider.Instance.GetParser<TId>();
            foreach (IEventPackage eventPackage in dbEvents.Events)
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

    public async Task<IAppendEventsResult> AppendToStream(ICommandEnvelope commandEnvelope, StreamId streamName, AggregateVersion aggregateVersion, IReadOnlyList<IEventEnvelope> events)
    {
        AppendCommandPackage commandPackage = new AppendCommandPackage()
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
        AppendEventPackage[] eventsPackage = events.Select(x => new AppendEventPackage()
        {
            Payload = x.Payload,
            Timestamp = x.Timestamp,
            EventId = x.EventId,
            StreamName = streamName
        }).ToArray();

        AppendDataPackage package = new AppendDataPackage(commandPackage, eventsPackage);

        IAppendEventsResult result = await _appender.Append(streamName, package, aggregateVersion);
        return result;
    }
}