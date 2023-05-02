using System;
using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Abstractions.Types;

namespace EventSourcing.Net.Abstractions.Contracts;

/// <summary>
/// Factory to get appender for specific tenant.
/// </summary>
public interface IResolveAppender
{
    /// <summary>
    /// Get tenant specific appender.
    /// </summary>
    /// <param name="tenantId">Tenant id.</param>
    /// <returns>Appender for specific tenant.</returns>
    IAppendOnly Get(TenantId tenantId);
}
    
/// <summary>
/// Interface for appender to append events to storage.
/// </summary>
public interface IAppendOnly : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Append events to the events storage.
    /// </summary>
    /// <param name="streamName">Name of the stream.</param>
    /// <param name="data">Events data.</param>
    /// <param name="expectedStreamVersion">Expected version of events stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of append operation.</returns>
    /// <exception cref="EventSourcing.Net.Engine.Exceptions.AppendOnlyStoreConcurrencyException">Event with the given version already presents in the stream.</exception>
    Task<IAppendEventsResult> Append<TId>(StreamId streamName, IAppendDataPackage<TId> data, AggregateVersion expectedStreamVersion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read events for specific stream.
    /// </summary>
    /// <param name="streamName">Stream name.</param>
    /// <param name="from">Position of first event.</param>
    /// <param name="to">Position of last event.</param>
    /// <returns>Events data.</returns>
    Task<IEventsData> ReadSpecificStream(StreamId streamName, StreamPosition from, StreamPosition to);

    /// <summary>
    /// Read events by given conditions.
    /// </summary>
    /// <param name="from">Position of first event.</param>
    /// <param name="to">Position of last event.</param>
    /// <param name="prefixPattern">Prefix of stream.</param>
    /// <returns>Events data.</returns>
    Task<IEventsData> ReadAllStreams(StreamPosition from, StreamPosition to, params string[] prefixPattern)
        => ReadAllStreams(new StreamReadOptions(from, to, filterType: AggregateStreamFilterType.Include,
            prefixPattern: prefixPattern));

    /// <summary>
    /// Read event by given conditions.
    /// </summary>
    /// <param name="readOptions">Search options.</param>
    /// <returns>Events data.</returns>
    Task<IEventsData> ReadAllStreams(StreamReadOptions readOptions);

    /// <summary>
    /// Return streams started with given prefix.
    /// </summary>
    /// <param name="startsWithPrefix">Prefix of streams.</param>
    /// <returns>Names of streams.</returns>
    Task<StreamId[]> FindStreamIds(string startsWithPrefix);

    /// <summary>
    /// Check whether storage exists.
    /// </summary>
    Task<bool> IsExist();

    /// <summary>
    /// Initialize the appender.
    /// </summary>
    Task Initialize();
}