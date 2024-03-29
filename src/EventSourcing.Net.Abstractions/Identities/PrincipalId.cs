// <auto-generated />
using System;
using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Net.Abstractions.Identities
{
    /// <summary>
    /// Principal identifier.
    /// </summary>
    /// <remarks>Generated from EventSourcing.Net.CodeGeneration.Identities.tt</remarks>
    public readonly struct PrincipalId : IIdentity, IEquatable<PrincipalId>
    {
        private readonly string _id;

        /// <summary>
        /// Get the prefix.
        /// </summary>
        public static readonly string Prefix = "principal_";

        /// <summary>
        /// Get empty PrincipalId.
        /// </summary>
        public static readonly PrincipalId Empty = new PrincipalId();

        /// <summary>
        /// Create PrincipalId with string value.
        /// </summary>
        public static PrincipalId New(string id)
        {
            return new PrincipalId(id);
        }

        /// <summary>
        /// Initiate new object.
        /// </summary>
        public PrincipalId(string id)
        {
            if (id == null)
            {
                Thrown.ArgumentNullException(nameof(id));
            }

            _id = id;
        }

        /// <summary>
        /// Parse from string.
        /// </summary>
        /// <param name="serializedId">String value.</param>
        /// <returns>Principal identifier.</returns>
        /// <exception cref="ArgumentException">String cannot be parsed to PrincipalId.</exception>
        public static PrincipalId Parse(string serializedId)
        {
            if (serializedId == null)
            {
                Thrown.ArgumentNullException(nameof(serializedId));
            }

            if (!serializedId.StartsWith(Prefix, StringComparison.Ordinal))
            {
                Thrown.ArgumentException("PrincipalId shouldn't be null and should starts with prefix " + Prefix, "serializedId");
            }

            return new PrincipalId(serializedId.Substring(Prefix.Length));
        }

        /// <summary>
        /// Parse from string without throwing exception.
        /// </summary>
        /// <param name="serializedId">String value.</param>
        /// <param name="id">Parsed Principal identifier.</param>
        /// <returns>True if parsed successfully, otherwise false.</returns>
        public static bool TryParse(string serializedId, out PrincipalId id)
        {
            if (!string.IsNullOrWhiteSpace(serializedId) && serializedId.StartsWith(Prefix, StringComparison.Ordinal))
            {
                id = PrincipalId.Parse(serializedId);
                return true;
            }

            id = default(PrincipalId);
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

        public bool Equals(PrincipalId other) { return string.Equals(_id, other._id, StringComparison.Ordinal); }

        public override bool Equals(object other)
        {
            if ((other == null) || !(other is PrincipalId)) return false;

            return this.Equals((PrincipalId)other);
        }

        public override int GetHashCode() { return _id.GetHashCode(); }

        public static bool operator ==(PrincipalId a, PrincipalId b) { return string.Equals(a._id, b._id, StringComparison.Ordinal); }
        public static bool operator !=(PrincipalId a, PrincipalId b) { return !string.Equals(a._id, b._id, StringComparison.Ordinal); }
    }
}
