using EventSourcing.Core.Contracts;

namespace EventSourcing.Core;

/// <summary>
/// Events data.
/// </summary>
/// <param name="Events">Events.</param>
/// <param name="StreamEndPosition">Information about stream end.</param>
public record EventsData(IEventPackage[] Events, StreamPosition StreamEndPosition) : IEventsData;