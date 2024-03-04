using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Abstractions.Types;

namespace EventSourcing.Net.Engine.InMemory;

/// <inheritdoc />
public sealed class InMemoryAppender : IAppendOnly
{
    private readonly Dictionary<StreamId, List<EventPackage>> _data = new();
    private readonly object _locker = new();

    public Task<IAppendEventsResult> Append<TId>(StreamId streamName, IAppendDataPackage<TId> data, AggregateVersion expectedStreamVersion, CancellationToken cancellationToken = default)
    {
        lock (_locker)
        {
            if (!_data.TryGetValue(streamName, out List<EventPackage>? items))
            {
                items = new List<EventPackage>();
                _data[streamName] = items;
            }

            int position = items.Count;
            foreach (IAppendEventPackage package in data.EventPackages)
            {
                position++;
                items.Add(new EventPackage()
                {
                    PrincipalId = data.CommandPackage.PrincipalId,
                    Payload = package.Payload,
                    Timestamp = package.Timestamp,
                    CommandId = data.CommandPackage.CommandId,
                    EventId = package.EventId,
                    SequenceId = data.CommandPackage.SequenceId,
                    StreamName = package.StreamName,
                    StreamPosition = position,
                    TenantId = data.CommandPackage.TenantId
                });
            }
            AppendEventsResult result = new AppendEventsResult(true, position);

            return Task.FromResult((IAppendEventsResult)result);
        }
    }

    public Task<IEventsData> ReadSpecificStream(StreamId streamName, StreamPosition from, StreamPosition to)
    {
        lock (_locker)
        {
            if (!_data.TryGetValue(streamName, out List<EventPackage>? data))
            {
                return Task.FromResult<IEventsData>(new EventsData(Array.Empty<EventPackage>(), StreamPosition.Begin));
            }

            return Task.FromResult<IEventsData>(new EventsData(data.Where(x => x.StreamPosition > from && x.StreamPosition <= to).Select(x => x).ToArray(), data.Count));
        }
    }

    public Task<IExtendedEventsData> ReadAllStreams(StreamReadOptions readOptions)
    {
        lock (_locker)
        {
            throw new NotSupportedException();
        }
    }

    public Task<StreamId[]> FindStreamIds(string startsWithPrefix)
    {
        lock (_locker)
        {
            return Task.FromResult(_data.Keys
                .Select(x => x.ToString())
                .Where(x => x.StartsWith(startsWithPrefix, StringComparison.OrdinalIgnoreCase))
                .Select(x => new StreamId(x))
                .ToArray());
        }
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
        // here is nothing to dispose
    }

    public async ValueTask DisposeAsync()
    {
        // here is nothing to dispose
    }
}