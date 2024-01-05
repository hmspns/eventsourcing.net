// <auto-generated />
using System;
using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Net.Samples.CustomIdTypes
{
    using System.ComponentModel;
    using TypeConverters;

    /// <summary>
    /// Guid based identifier.
    /// </summary>
    /// <remarks>Generated from SpeedDating.CodeGeneration.Identities.tt</remarks>
    [TypeConverter(typeof(GuidIdConverter))]
    public readonly struct GuidId : IEquatable<GuidId>
    {
        private readonly Guid _id;

        /// <summary>
        /// Get the prefix.
        /// </summary>
        public static readonly string Prefix = "guid_";

        /// <summary>
        /// Get empty GuidId.
        /// </summary>
        public static readonly GuidId Empty = new GuidId(Guid.Empty);

        /// <summary>
        /// Create GuidId with random value.
        /// </summary>
        public static GuidId New()
        {
            return new GuidId(Guid.NewGuid());
        }

        /// <summary>
        /// Initiate new object.
        /// </summary>
        public GuidId(Guid id)
        {
            _id = id;
        }

        /// <summary>
        /// Parse from string.
        /// </summary>
        /// <param name="serializedId">String value.</param>
        /// <returns>Guid based identifier.</returns>
        /// <exception cref="ArgumentException">String cannot be parsed to GuidId.</exception>
        public static GuidId Parse(string serializedId)
        {
            if (!serializedId.StartsWith(Prefix, StringComparison.Ordinal))
            {
                throw new ArgumentException("Invalid GuidId", "serializedId");
            }

            ReadOnlySpan<char> span = serializedId.AsSpan();
            Guid id = Guid.Parse(span.Slice(Prefix.Length));

            return new GuidId(id);
        }

        /// <summary>
        /// Parse from string without throwing exception.
        /// </summary>
        /// <param name="serializedId">String value.</param>
        /// <param name="id">Parsed GuidId.</param>
        /// <returns>True if parsed successfully, otherwise false.</returns>
        public static bool TryParse(string serializedId, out GuidId id)
        {
            Guid guid;

            if (!string.IsNullOrWhiteSpace(serializedId) && serializedId.StartsWith(Prefix, StringComparison.Ordinal))
            {
                ReadOnlySpan<char> span = serializedId.AsSpan().Slice(Prefix.Length);
                if (Guid.TryParse(span, out guid))
                {
                    id = new GuidId(guid);
                    return true;
                }
            }

            id = default(GuidId);
            return false;
        }

        /// <summary>
        /// Get internal id.
        /// </summary>
        public Guid Id => _id;

        public static implicit operator Guid(GuidId id)
        {
            return id._id;
        }

        public static implicit operator GuidId(Guid id)
        {
            return new GuidId(id);
        }

        public override string ToString()
        {
            return string.Concat(Prefix, _id.ToString());
        }

        public bool Equals(GuidId other) { return _id == other._id; }

        public override bool Equals(object other)
        {
            if ((other == null) || !(other is GuidId)) 
            {
                return false;
            }

            return this.Equals((GuidId)other);
        }

        public override int GetHashCode() { return _id.GetHashCode(); }

        public static bool operator ==(GuidId a, GuidId b) { return a._id == b._id; }
        public static bool operator !=(GuidId a, GuidId b) { return a._id != b._id; }
    }
}
