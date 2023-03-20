using System.Collections.Generic;
using EventSourcing.Abstractions;
using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Core
{
    /// <summary>
    /// Events stream.
    /// </summary>
    public sealed class EventsStream
    {
        /// <summary>
        /// Name of stream.
        /// </summary>
        public StreamId StreamName { get; init; }
        
        /// <summary>
        /// Start position.
        /// </summary>
        public StreamPosition From { get; init; }
        
        /// <summary>
        /// End position.
        /// </summary>
        public StreamPosition To { get; init; }
        
        /// <summary>
        /// Whether stream contains events until the end of the stream.
        /// </summary>
        public bool IsEndOfStream { get; init; }
        
        /// <summary>
        /// Whether stream contains events.
        /// </summary>
        public bool HasEvents { get; init; }
        
        /// <summary>
        /// Stream version.
        /// </summary>
        public AggregateVersion Version { get; init; }
        
        /// <summary>
        /// Stream events.
        /// </summary>
        public IReadOnlyList<IEventEnvelope> Events { get; init; }
    }
}