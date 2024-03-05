namespace EventSourcing.Net.Abstractions;

/// <summary>
/// Provide status for event sourcing.
/// </summary>
public interface IEventSourcingStatus
{
    /// <summary>
    /// Is event sourcing engine started.
    /// </summary>
    public bool IsStarted { get; }
}