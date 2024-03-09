using System;
using System.Buffers;
using System.Diagnostics;
using System.Threading.Tasks;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Abstractions.Types;
using EventSourcing.Net.Engine.Extensions;

namespace EventSourcing.Net.Engine.Rebuild;

using Abstractions;

/// <summary>
/// Build views based on events stream.
/// </summary>
public sealed class ViewsRebuilder : IViewsRebuilder
{
    private readonly IResolveAppender _resolveAppender;
    private readonly IResolveEventPublisher _resolveEventPublisher;
    private readonly ITypeMappingHandler _typeMappingHandler;
    private readonly IEventSourcingStatus _status;

    /// <summary>
    /// Raised when rebuild of batch done.
    /// </summary>
    public event ViewsBatchRebuiltEventHandler OnBatchRebuilt;
    
    public ViewsRebuilder(
        IResolveAppender resolveAppender,
        IResolveEventPublisher resolveEventPublisher,
        ITypeMappingHandler typeMappingHandler,
        IEventSourcingStatus status)
    {
        _status = status;
        _typeMappingHandler = typeMappingHandler;
        _resolveEventPublisher = resolveEventPublisher;
        _resolveAppender = resolveAppender;
    }

    /// <summary>
    /// Rebuild views for default tenant.
    /// </summary>
    /// <param name="batchSize">Batch size for iteration.</param>
    public Task Rebuild(int batchSize = 1000)
    {
        return RebuildTenant(TenantId.Empty, batchSize);
    }
    
    /// <summary>
    /// Rebuild views for tenant.
    /// </summary>
    /// <param name="tenantId">Tenant id.</param>
    /// <param name="batchSize">Batch size for iteration.</param>
    public async Task RebuildTenant(TenantId tenantId, int batchSize = 1000)
    {
        if (!_status.IsStarted)
        {
            Exceptions.Thrown.InvalidOperationException("Event sourcing engine not started");
        }
        
        StreamPosition position = StreamPosition.Begin;

        IExtendedEventsData data = null;
        try
        {
            do
            {
                Stopwatch st = Stopwatch.StartNew();
                TraceHelper th = new TraceHelper(
                    $"Reading batch from position {position.ToString()}",
                    $"Batch read from {position.ToString()}");
                data = await ReadBatch(tenantId, position, batchSize);
                th.Dispose(); // to avoid second message on exception
                
                if (data.Events.Count == 0)
                {
                    Trace.WriteLine("Rebuild done");
                    break;
                }

                IEventPublisher publisher = _resolveEventPublisher.Get(tenantId);
            
                th = new TraceHelper(
                    $"Creating views for {data.Events.Count.ToString()} events",
                    "Views created");
                await BuildViews(publisher, data);
                th.Dispose(); // to avoid second message on exception
                
                st.Stop();
                
                OnBatchRebuilt?.Invoke(this, new ViewsBatchRebuiltEventArgs()
                {
                    BatchSize = batchSize,
                    StartPosition = position,
                    EndPosition = position + batchSize - 1,
                    EventsProcessed = data.Events.Count,
                    TimeElapsed = st.Elapsed
                });
                
                position += batchSize;
            } 
            while (data.Events.Count > 0);
        }
        finally
        {
            data?.Dispose();
        }
    }

    /// <summary>
    /// Read batch of data.
    /// </summary>
    /// <param name="tenantId">Tenant id.</param>
    /// <param name="from">Start position.</param>
    /// <param name="count">Count of events to be read.</param>
    /// <returns>Batch of events data.</returns>
    private async Task<IExtendedEventsData> ReadBatch(TenantId tenantId, StreamPosition from, int count)
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

        IExtendedEventsData events = await appender.ReadAllStreams(readOptions).ConfigureAwait(false);

        return events;
    }

    /// <summary>
    /// Send events to the bus to generate views.
    /// </summary>
    /// <param name="events">Events that should be added to the bus.</param>
    private async Task BuildViews(IEventPublisher eventPublisher, IExtendedEventsData events)
    {
        IEventEnvelope[] buffer = null;
        TypeMappingId typeMappingId = TypeMappingId.Empty;
        Type aggregateIdType = null;
        Func<string, object> parser = null;
        try
        {
            buffer = ArrayPool<IEventEnvelope>.Shared.Rent(events.Events.Count);
            int index = 0;
            
            foreach (ExtendedEventPackage eventPackage in events.Events)
            {
                SetTypeId(eventPackage.AggregateTypeId);    
                
                buffer[index] = eventPackage.ToEventEnvelope(parser);
                index++;
            }
            ArraySegment<IEventEnvelope> segment = new ArraySegment<IEventEnvelope>(buffer, 0, events.Events.Count);

            await eventPublisher.Publish(null, segment).ConfigureAwait(false);
        }
        finally
        {
            if (buffer != null)
            {
                ArrayPool<IEventEnvelope>.Shared.Return(buffer);
            }
        }
 
        void SetTypeId(Guid aggregateTypeId)
        {
            TypeMappingId localTypeMappingId = new TypeMappingId(aggregateTypeId);
            if (localTypeMappingId != typeMappingId)
            {
                typeMappingId = localTypeMappingId;
                aggregateIdType = _typeMappingHandler.GetTypeById(typeMappingId);
                parser = AggregateIdParsingProvider.Instance.GetParser(aggregateIdType);
            }
        }
    }
}