// <auto-generated />
using System;
using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Net.Samples.CustomIdTypes
{
    using System.ComponentModel;
    using TypeConverters;

    /// <summary>
    /// String based identifier.
    /// </summary>
    /// <remarks>Generated from SpeedDating.CodeGeneration.Identities.tt</remarks>
    [TypeConverter(typeof(StringIdConverter))]
    public readonly struct StringId : IEquatable<StringId>
    {
        private readonly string _id;

        /// <summary>
        /// Get the prefix.
        /// </summary>
        public static readonly string Prefix = "string_";

        /// <summary>
        /// Get empty StringId.
        /// </summary>
        public static readonly StringId Empty = new StringId();

        /// <summary>
        /// Create StringId with string value.
        /// </summary>
        public static StringId New(string id)
        {
            return new StringId(id);
        }

        /// <summary>
        /// Initiate new object.
        /// </summary>
        public StringId(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            _id = id;
        }

        /// <summary>
        /// Parse from string.
        /// </summary>
        /// <param name="serializedId">String value.</param>
        /// <returns>String based identifier.</returns>
        /// <exception cref="ArgumentException">String cannot be parsed to StringId.</exception>
        public static StringId Parse(string serializedId)
        {
            if (serializedId == null)
            {
                throw new ArgumentNullException(nameof(serializedId));
            }

            if (!serializedId.StartsWith(Prefix, StringComparison.Ordinal))
            {
                throw new ArgumentException("Invalid StringId", "serializedId");
            }

            return new StringId(serializedId.Substring(Prefix.Length));
        }

        /// <summary>
        /// Parse from string without throwing exception.
        /// </summary>
        /// <param name="serializedId">String value.</param>
        /// <param name="id">Parsed String based identifier.</param>
        /// <returns>True if parsed successfully, otherwise false.</returns>
        public static bool TryParse(string serializedId, out StringId id)
        {
            if (!string.IsNullOrWhiteSpace(serializedId) && serializedId.StartsWith(Prefix, StringComparison.Ordinal))
            {
                id = StringId.Parse(serializedId);
                return true;
            }

            id = default(StringId);
            return false;
        }

        /// <summary>
        /// Get internal id.
        /// </summary>
        public string Id => _id;

        public override string ToString()
        {
            return Prefix + _id;
        }

        public bool Equals(StringId other) { return string.Equals(_id, other._id, StringComparison.Ordinal); }

        public override bool Equals(object other)
        {
            if ((other == null) || !(other is StringId)) return false;

            return this.Equals((StringId)other);
        }

        public override int GetHashCode() { return _id.GetHashCode(); }

        public static bool operator ==(StringId a, StringId b) { return string.Equals(a._id, b._id, StringComparison.Ordinal); }
        public static bool operator !=(StringId a, StringId b) { return !string.Equals(a._id, b._id, StringComparison.Ordinal); }
    }
}
