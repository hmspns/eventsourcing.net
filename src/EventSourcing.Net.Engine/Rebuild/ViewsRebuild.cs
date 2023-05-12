using System;
using System.Diagnostics;
using System.Threading.Tasks;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Abstractions.Types;

namespace EventSourcing.Net.Engine.Rebuild;

/// <summary>
/// Build views based on events stream.
/// </summary>
public sealed class ViewsRebuilder
{
    private readonly IResolveAppender _resolveAppender;
    private readonly IEventBus _eventBus;
    private readonly int _batchSize;

    public ViewsRebuilder(
        IResolveAppender resolveAppender,
        IEventBus eventBus,
        int batchSize = 1000)
    {
        _batchSize = batchSize;
        _eventBus = eventBus;
        _resolveAppender = resolveAppender;
    }
    
    /// <summary>
    /// Rebuild views for tenant.
    /// </summary>
    /// <param name="tenantId">Tenant id.</param>
    public async Task RebuildTenant(TenantId tenantId)
    {
        StreamPosition position = StreamPosition.Begin;

        IEventsData data;
        do
        {
            TraceHelper th = new TraceHelper(
                $"Reading batch from position {position.ToString()}",
                $"Batch read from {position.ToString()}");
            data = await ReadBatch(tenantId, position, _batchSize).ConfigureAwait(false);
            th.Dispose(); // to avoid second message on exception


            if (data.Events.Count == 0)
            {
                Trace.WriteLine("Rebuild done");
                break;
            }

            th = new TraceHelper(
                $"Creating views for {data.Events.Count.ToString()} events",
                "Views created");
            await BuildViews(data).ConfigureAwait(false);
            th.Dispose(); // to avoid second message on exception

            position = position + _batchSize;
        } 
        while (data.Events.Count > 0);
    }

    /// <summary>
    /// Read batch of data.
    /// </summary>
    /// <param name="tenantId">Tenant id.</param>
    /// <param name="from">Start position.</param>
    /// <param name="count">Count of events to be read.</param>
    /// <returns>Batch of events data.</returns>
    private async Task<IEventsData> ReadBatch(TenantId tenantId, StreamPosition from, int count)
    {
        IAppendOnly appender = _resolveAppender.Get(tenantId);

        StreamReadOptions readOptions = new StreamReadOptions()
        {
            ReadDirection = StreamReadDirection.Forward,
            From = from,
            To = from.Ahead(count),
            StoragePositionType = StoragePositionType.GlobalPosition,
            ReadingVolume = StreamReadVolume.MetaAndData
        };

        IEventsData events = await appender.ReadAllStreams(readOptions).ConfigureAwait(false);

        return events;
    }

    /// <summary>
    /// Send events to the bus to generate views.
    /// </summary>
    /// <param name="events">Events that should be added to the bus.</param>
    private async Task BuildViews(IEventsData events)
    {
        foreach (EventPackage eventPackage in events.Events)
        {
            throw new NotImplementedException();
            //IEventEnvelope e = eventPackage.ToEventEnvelope();
            //await _eventBus.Send(e);
        }
    }
}