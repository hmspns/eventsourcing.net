using EventSourcing.Abstractions.Types;

namespace EventSourcing.Abstractions.Contracts;

/// <summary>
/// Append events result.
/// </summary>
public interface IAppendEventsResult
{
    /// <summary>
    /// Whether append operation was successfully.
    /// </summary>
    bool IsSuccess { get; }
        
    /// <summary>
    /// Get version of latest event.
    /// </summary>
    AggregateVersion Version { get; }
}

/// <inheritdoc />
public record AppendEventsResult(bool IsSuccess, AggregateVersion Version) : IAppendEventsResult;