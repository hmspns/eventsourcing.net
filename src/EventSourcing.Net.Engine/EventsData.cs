using System.Collections.Generic;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Types;

namespace EventSourcing.Net.Engine;

using System;

/// <summary>
/// Events data.
/// </summary>
/// <param name="Events">Events.</param>
/// <param name="StreamEndPosition">Information about stream end.</param>
public sealed record EventsData(IReadOnlyCollection<EventPackage> Events, StreamPosition StreamEndPosition) : IEventsData
{
    /// <inheritdoc />
    public void Dispose()
    {
        if (Events is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}