namespace EventSourcing.Net.Samples.Rebuild;

using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Abstractions.Types;
using EventSourcing.Net.Engine;

public class VirtualAppenderResolver : IResolveAppender
{
    private readonly RebuildAppender _appender;

    public VirtualAppenderResolver(TypeMappingId typeMappingId)
    {
        _appender = new RebuildAppender(typeMappingId);
    }
    
    public IAppendOnly Get(TenantId tenantId)
    {
        return _appender;
    }
}

public class RebuildAppender : IAppendOnly
{
    private readonly TypeMappingId _packageTypeId;
    
    private long _position = 0;

    private readonly List<ExtendedEventPackage> _events = new List<ExtendedEventPackage>();
    
    public RebuildAppender(TypeMappingId typeMappingId)
    {
        _packageTypeId = typeMappingId;
    }

    public async Task<IAppendEventsResult> Append<TId>(StreamId streamName, IAppendDataPackage<TId> data, AggregateVersion expectedStreamVersion, CancellationToken cancellationToken = default)
    {
        long position = -1;
        foreach (IAppendEventPackage package in data.EventPackages)
        {
            position = Interlocked.Increment(ref _position);
            _events.Add(new ExtendedEventPackage()
            {
                PrincipalId = data.CommandPackage.PrincipalId,
                Payload = package.Payload,
                Timestamp = package.Timestamp,
                CommandId = data.CommandPackage.CommandId,
                EventId = package.EventId,
                SequenceId = data.CommandPackage.SequenceId,
                StreamName = package.StreamName,
                StreamPosition = position,
                TenantId = data.CommandPackage.TenantId,
                AggregateTypeId = _packageTypeId,
            });
        }
        AppendEventsResult result = new AppendEventsResult(true, position);

        return result;
    }

    public Task<IEventsData> ReadSpecificStream(StreamId streamName, StreamPosition from, StreamPosition to)
    {
        IEnumerable<EventPackage> events = _events
                                           .Where(x => x.StreamName == streamName && x.StreamPosition >= from && x.StreamPosition <= to)
                                           .Select(x => new EventPackage()
                                            {
                                                CommandId = x.CommandId,
                                                EventId = x.EventId,
                                                Payload = x.Payload,
                                                SequenceId = x.SequenceId,
                                                StreamName = x.StreamName,
                                                StreamPosition = x.StreamPosition,
                                                Timestamp = x.Timestamp,
                                                TenantId = x.TenantId,
                                                PrincipalId = x.PrincipalId
                                            });
        EventsData data = new EventsData(events.ToArray(), _events.Count);
        return Task.FromResult((IEventsData)data);
    }

    public Task<IExtendedEventsData> ReadAllStreams(StreamReadOptions readOptions)
    {
        ExtendedEventPackage[] matchedEvents = _events
                                               .Where(x => x.StreamPosition >= readOptions.From && x.StreamPosition <= readOptions.To)
                                               .ToArray();
        
        StreamPosition streamEndPosition = matchedEvents.LastOrDefault().StreamPosition;
        
        return Task.FromResult((IExtendedEventsData)new ExtendedEventsData(matchedEvents, streamEndPosition));
    }

    public Task<StreamId[]> FindStreamIds(string startsWithPrefix)
    {
        StreamId[] events = _events
                            .Where(x => x.StreamName.Id.StartsWith(startsWithPrefix))
                            .Select(x => x.StreamName)
                            .ToArray();
        return Task.FromResult(events);
    }

    public Task<bool> IsExist()
    {
        return Task.FromResult(true);
    }

    public Task Initialize()
    {
        return Task.CompletedTask;
    }
    
    public void Dispose()
    {
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}