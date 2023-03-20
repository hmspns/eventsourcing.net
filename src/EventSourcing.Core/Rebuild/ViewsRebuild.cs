using System.Threading.Tasks;
using EventSourcing.Abstractions;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Core.Contracts;
using EventSourcing.Core.Extensions;

namespace EventSourcing.Core.Rebuild;

/// <summary>
/// Build views based on events stream.
/// </summary>
public sealed class ViewsRebuilder
{
    private readonly IResolveAppender _resolveAppender;
    private readonly IEventBus _eventBus;
    private readonly ILoggerFactory _loggerFactory;
    private readonly int _batchSize;

    public ViewsRebuilder(
        IResolveAppender resolveAppender,
        IEventBus eventBus,
        ILoggerFactory loggerFactory,
        int batchSize = 1000)
    {
        _batchSize = batchSize;
        _loggerFactory = loggerFactory;
        _eventBus = eventBus;
        _resolveAppender = resolveAppender;
    }
    
    /// <summary>
    /// Rebuild views for tenant.
    /// </summary>
    /// <param name="tenantId">Tenant id.</param>
    public async Task RebuildTenant(TenantId tenantId)
    {
        var logger = _loggerFactory
            .CreateLogger<ViewsRebuilder>()
            .WithProperty("TenantId", tenantId.ToString());
        StreamPosition position = StreamPosition.Begin;

        IEventsData data;
        do
        {
            using (ITimedOperation operation = logger.StartTimedOperation(LogLevel.Verbose, "Read batch from {position}.", position))
            {
                data = await ReadBatch(tenantId, position, _batchSize);
            }

            if (data.Events.Length == 0)
            {
                logger.Verbose("Rebuild done");
                break;
            }

            using (ITimedOperation operation = logger.StartTimedOperation(LogLevel.Verbose, "Created views for {count} events", data.Events.Length.ToString()))
            {
                await BuildViews(data);
            }

            position = position + _batchSize;
        } 
        while (data.Events.Length > 0);
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

        IEventsData events = await appender.ReadAllStreams(readOptions);

        return events;
    }

    /// <summary>
    /// Send events to the bus to generate views.
    /// </summary>
    /// <param name="events">Events that should be added to the bus.</param>
    private async Task BuildViews(IEventsData events)
    {
        foreach (IEventPackage eventPackage in events.Events)
        {
            IEventEnvelope e = eventPackage.ToEventEnvelope();
            await _eventBus.Send(e);
        }
    }
}