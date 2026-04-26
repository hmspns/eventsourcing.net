namespace EventSourcing.Net.Telemetry.Services;

using System.Collections.Concurrent;
using Contracts;

public sealed class PublicationTelemetryService : IPublicationTelemetryService
{
    private readonly ConcurrentDictionary<Type, TimeSpan> _preloadDataTelemetry = new ConcurrentDictionary<Type, TimeSpan>();
    private readonly ConcurrentDictionary<Type, TimeSpan> _saveDataTelemetry = new ConcurrentDictionary<Type, TimeSpan>();
    
    private readonly ConcurrentDictionary<Key, Value> _telemetry = new ConcurrentDictionary<Key, Value>();

    /// <summary>
    /// Added telemetry about preload data.
    /// </summary>
    /// <param name="consumerType">Type of the consumer.</param>
    /// <param name="duration">Publication duration.</param>
    public void AddPreloadDataTelemetry(Type consumerType, TimeSpan duration)
    {
        _preloadDataTelemetry.TryAdd(consumerType, duration);
    }

    /// <summary>
    /// Added telemetry about event publication.
    /// </summary>
    /// <param name="envelopeType">Type of the event envelope.</param>
    /// <param name="payloadType">Type of the payload.</param>
    /// <param name="consumerType">Type of the consumer.</param>
    /// <param name="duration">Publication duration.</param>
    public void AddTelemetry(Type envelopeType, Type payloadType, Type consumerType, TimeSpan duration)
    {
        _telemetry.AddOrUpdate(new Key(envelopeType, consumerType),
            new Value(payloadType, 1, duration),
            (key, value) =>
            {
                value.Update(duration);
                return value;
            });
    }

    public void AddSaveTelemetry(Type consumerType, TimeSpan duration)
    {
        _saveDataTelemetry.TryAdd(consumerType, duration);
    }

    /// <summary>
    /// Retrieves telemetry data for a specific envelope and consumer type combination.
    /// </summary>
    /// <param name="envelopeType">The type of the envelope</param>
    /// <param name="consumerType">The type of the consumer</param>
    /// <returns>Telemetry data containing average duration and payload type information</returns>
    public PublicationTelemetryData GetTelemetry(Type envelopeType, Type consumerType)
    {
        Key key = new Key(envelopeType, consumerType);
        if(!_telemetry.TryGetValue(key, out Value? value))
        {
            return new PublicationTelemetryData(envelopeType, null, consumerType, TimeSpan.Zero, 0);
        }
        
        return new PublicationTelemetryData(envelopeType, value.PayloadType, consumerType, value.AverageDuration, value.Count);
    }

    /// <summary>
    /// Retrieves all available telemetry data for all envelope and consumer type combinations.
    /// </summary>
    /// <returns>Collection of telemetry data entries</returns>
    public IEnumerable<PublicationTelemetryData> GetTelemetry()
    {
        foreach (KeyValuePair<Key, Value> pair in _telemetry)
        {
            yield return new PublicationTelemetryData(pair.Key.EnvelopeType, pair.Value.PayloadType, pair.Key.ConsumerType, pair.Value.AverageDuration, pair.Value.Count);
        }
    }

    /// <summary>
    /// Get preload telemetry.
    /// </summary>
    /// <remarks>Key is the type of consumer. Value is publication duration.</remarks>
    public IReadOnlyDictionary<Type, TimeSpan> PreloadDataTelemetry => _preloadDataTelemetry.AsReadOnly();

    /// <summary>
    /// Get save telemetry.
    /// </summary>
    /// <remarks>Key is the type of consumer. Value is saving duration.</remarks>
    public IReadOnlyDictionary<Type, TimeSpan> SaveDataTelemetry => _saveDataTelemetry.AsReadOnly();

    /// <summary>
    /// Clear all telemetry data.
    /// </summary>
    public void Clear()
    {
        _telemetry.Clear();
    }

    /// <summary>
    /// Represents a key for telemetry data storage.
    /// </summary>
    /// <param name="EnvelopeType">The type of the event envelope.</param>
    /// <param name="ConsumerType">The type of the event consumer.</param>
    private readonly record struct Key(Type EnvelopeType, Type ConsumerType);

    /// <summary>
    /// Represents telemetry values for event processing.
    /// </summary>
    private record Value
    {
        /// <summary>
        /// Initializes a new instance of the Value record.
        /// </summary>
        /// <param name="payloadType">The type of the event payload.</param>
        /// <param name="count">Initial count of processed events.</param>
        /// <param name="averageDuration">Initial average duration of event processing.</param>
        public Value(Type payloadType, int count, TimeSpan averageDuration)
        {
            PayloadType = payloadType;
            Count = count;
            AverageDuration = averageDuration;
        }

        /// <summary>
        /// Gets the number of processed events.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets the average duration of event processing.
        /// </summary>
        public TimeSpan AverageDuration { get; private set; }

        /// <summary>
        /// Gets the type of the event payload.
        /// </summary>
        public Type PayloadType { get; init; }

        /// <summary>
        /// Updates the telemetry with a new duration value.
        /// </summary>
        /// <param name="duration">The duration to include in the average calculation.</param>
        public void Update(TimeSpan duration)
        {
            long totalTicks = AverageDuration.Ticks * Count;
            totalTicks += duration.Ticks;
            Count++;
            AverageDuration = new TimeSpan(totalTicks / Count);
        }
    }
}

/// <summary>
/// Represents telemetry data for event publication.
/// </summary>
/// <param name="EnvelopeType">The type of the event envelope.</param>
/// <param name="PayloadType">The type of the event payload.</param>
/// <param name="ConsumerType">The type of the event consumer.</param>
/// <param name="AverageDuration">The average duration of event processing.</param>
/// <param name="Count">Count handled events.</param>
public readonly record struct PublicationTelemetryData(Type EnvelopeType, Type? PayloadType, Type ConsumerType, TimeSpan AverageDuration, int Count)
{
    /// <summary>
    /// Get the total duration of processing.
    /// </summary>
    public TimeSpan TotalDuration => AverageDuration * Count;
}