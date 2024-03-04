namespace EventSourcing.Net.Engine;

using System;
using System.Collections.Generic;
using Abstractions.Contracts;
using Abstractions.Types;

/// <summary>
/// Extended events data.
/// </summary>
/// <param name="Events">Events.</param>
/// <param name="StreamEndPosition">Information about stream end.</param>
public sealed record ExtendedEventsData(IReadOnlyList<ExtendedEventPackage> Events, StreamPosition StreamEndPosition) : IExtendedEventsData
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