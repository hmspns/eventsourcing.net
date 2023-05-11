using System.Collections.Generic;
using EventSourcing.Net.Abstractions.Types;

namespace EventSourcing.Net.Abstractions.Contracts;

/// <summary>
/// Data of events from storage.
/// </summary>
public interface IEventsData
{
    /// <summary>
    /// Events.
    /// </summary>
    public IReadOnlyCollection<EventPackage> Events { get; }
        
    /// <summary>
    /// Stream end position.
    /// </summary>
    public StreamPosition StreamEndPosition { get; }
}