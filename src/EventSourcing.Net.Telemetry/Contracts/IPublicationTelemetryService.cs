namespace EventSourcing.Net.Telemetry.Contracts;

using Services;

/// <summary>
/// Service to store publication telemetry data.
/// </summary>
public interface IPublicationTelemetryService
{
    /// <summary>
    /// Added telemetry about event publication.
    /// </summary>
    /// <param name="envelopeType">Type of the event envelope.</param>
    /// <param name="payloadType">Type of the payload.</param>
    /// <param name="consumerType">Type of the consumer.</param>
    /// <param name="duration">Publication duration.</param>
    void AddTelemetry(Type envelopeType, Type payloadType, Type consumerType, TimeSpan duration);

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
    /// Clear all telemetry data.
    /// </summary>
    void Clear();
}