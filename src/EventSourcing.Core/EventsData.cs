using System.Collections.Generic;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Types;

namespace EventSourcing.Core;

/// <summary>
/// Events data.
/// </summary>
/// <param name="Events">Events.</param>
/// <param name="StreamEndPosition">Information about stream end.</param>
public record EventsData(IReadOnlyCollection<IEventPackage> Events, StreamPosition StreamEndPosition) : IEventsData;