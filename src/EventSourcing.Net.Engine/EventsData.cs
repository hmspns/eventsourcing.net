using System.Collections.Generic;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Types;

namespace EventSourcing.Net.Engine;

/// <summary>
/// Events data.
/// </summary>
/// <param name="Events">Events.</param>
/// <param name="StreamEndPosition">Information about stream end.</param>
public record EventsData(IReadOnlyCollection<IEventPackage> Events, StreamPosition StreamEndPosition) : IEventsData;