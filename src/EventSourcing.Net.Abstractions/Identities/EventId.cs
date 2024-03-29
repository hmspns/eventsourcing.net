// <auto-generated />
using System;
using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Net.Abstractions.Identities
{
    /// <summary>
    /// Event identifier.
    /// </summary>
    /// <remarks>Generated from EventSourcing.Net.CodeGeneration.Identities.tt</remarks>
    public readonly struct EventId : IIdentity, IEquatable<EventId>
    {
        private readonly Guid _id;

        /// <summary>
        /// Get the prefix.
        /// </summary>
        public static readonly string Prefix = "event_";

        /// <summary>
        /// Get empty EventId.
        /// </summary>
        public static readonly EventId Empty = new EventId(Guid.Empty);

        /// <summary>
        /// Create EventId with random value.
        /// </summary>
        public static EventId New()
        {
            return new EventId(Guid.NewGuid());
        }

        /// <summary>
        /// Initiate new object.
        /// </summary>
        public EventId(Guid id)
        {
            _id = id;
        }

        /// <summary>
        /// Parse from string.
        /// </summary>
        /// <param name="serializedId">String value.</param>
        /// <returns>Event identifier.</returns>
        /// <exception cref="ArgumentException">String cannot be parsed to EventId.</exception>
        public static EventId Parse(string serializedId)
        {
            if (serializedId?.StartsWith(Prefix, StringComparison.Ordinal) != true)
            {
                Thrown.ArgumentException("EventId shouldn't be null and should starts with prefix " + Prefix, nameof(serializedId));
            }

            ReadOnlySpan<char> span = serializedId.AsSpan();
            Guid id = Guid.Parse(span.Slice(Prefix.Length));

            return new EventId(id);
        }

        /// <summary>
        /// Parse from string without throwing exception.
        /// </summary>
        /// <param name="serializedId">String value.</param>
        /// <param name="id">Parsed EventId.</param>
        /// <returns>True if parsed successfully, otherwise false.</returns>
        public static bool TryParse(string serializedId, out EventId id)
        {
            Guid guid;

            if (!string.IsNullOrWhiteSpace(serializedId) && serializedId.StartsWith(Prefix, StringComparison.Ordinal))
            {
                ReadOnlySpan<char> span = serializedId.AsSpan().Slice(Prefix.Length);
                if (Guid.TryParse(span, out guid))
                {
                    id = new EventId(guid);
                    return true;
                }
            }

            id = default(EventId);
            return false;
        }

        /// <summary>
        /// Get internal id.
        /// </summary>
        public Guid Id => _id;

        public static implicit operator Guid(EventId id)
        {
            return id._id;
        }

        public static implicit operator EventId(Guid id)
        {
            return new EventId(id);
        }

        public override string ToString()
        {
            return string.Concat(Prefix, _id.ToString());
        }

        public bool Equals(EventId other) { return _id == other._id; }

        public override bool Equals(object other)
        {
            if ((other == null) || !(other is EventId)) 
            {
                return false;
            }

            return this.Equals((EventId)other);
        }

        public override int GetHashCode() { return _id.GetHashCode(); }

        public static bool operator ==(EventId a, EventId b) { return a._id == b._id; }
        public static bool operator !=(EventId a, EventId b) { return a._id != b._id; }
    }
}
