using System.Collections.Generic;
using EventSourcing.Net.Abstractions.Types;

namespace EventSourcing.Net.Abstractions.Contracts;

using System;

/// <summary>
/// Data of events from storage.
/// </summary>
public interface IEventsData : IDisposable
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