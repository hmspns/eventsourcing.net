namespace EventSourcing.Net.Telemetry.Contracts;

using Services;

/// <summary>
/// Service to store publication telemetry data.
/// </summary>
public interface IPublicationTelemetryService
{
    /// <summary>
    /// Added telemetry about preload data.
    /// </summary>
    /// <param name="consumerType">Type of the consumer.</param>
    /// <param name="duration">Publication duration.</param>
    void AddPreloadDataTelemetry(Type consumerType, TimeSpan duration);
    
    /// <summary>
    /// Added telemetry about event publication.
    /// </summary>
    /// <param name="envelopeType">Type of the event envelope.</param>
    /// <param name="payloadType">Type of the payload.</param>
    /// <param name="consumerType">Type of the consumer.</param>
    /// <param name="duration">Publication duration.</param>
    void AddTelemetry(Type envelopeType, Type payloadType, Type consumerType, TimeSpan duration);

    /// <summary>
    /// Added telemetry about save data.
    /// </summary>
    /// <param name="consumerType">Type of the consumer.</param>
    /// <param name="duration">Publication duration.</param>
    void AddSaveTelemetry(Type consumerType, TimeSpan duration);
    
    /// <summary>
    /// Retrieves telemetry data for a specific envelope and consumer type combination.
    /// </summary>
    /// <param name="envelopeType">The type of the envelope</param>
    /// <param name="consumerType">The type of the consumer</param>
    /// <returns>Telemetry data containing average duration and payload type information</returns>
    PublicationTelemetryData GetTelemetry(Type envelopeType, Type consumerType);

    /// <summary>
    /// Retrieves all available telemetry data for all envelope and consumer type combinations.
    /// </summary>
    /// <returns>Collection of telemetry data entries</returns>
    IEnumerable<PublicationTelemetryData> GetTelemetry();
    
    /// <summary>
    /// Get preload telemetry.
    /// </summary>
    /// <remarks>Key is the type of consumer. Value is publication duration.</remarks>
    public IReadOnlyDictionary<Type, TimeSpan> PreloadDataTelemetry { get; }
    
    /// <summary>
    /// Get save telemetry.
    /// </summary>
    /// <remarks>Key is the type of consumer. Value is saving duration.</remarks>
    public IReadOnlyDictionary<Type, TimeSpan> SaveDataTelemetry { get; }
    
    /// <summary>
    /// Clear all telemetry data.
    /// </summary>
    void Clear();
}