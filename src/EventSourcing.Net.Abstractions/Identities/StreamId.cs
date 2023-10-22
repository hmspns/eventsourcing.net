// <auto-generated />
using System;
using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Net.Abstractions.Identities
{
    /// <summary>
    /// Identifier of stream.
    /// </summary>
    /// <remarks>Generated from EventSourcing.Net.CodeGeneration.</remarks>
    public readonly struct StreamId : IIdentity, IEquatable<StreamId>
    {
        private readonly string _id;

        /// <summary>
        /// Get the prefix.
        /// </summary>
        public static readonly StreamId Empty = new StreamId();

        /// <summary>
        /// Get empty StreamId.
        /// </summary>
        public static StreamId New(string id)
        {
            return new StreamId(id);
        }

        /// <summary>
        /// Create StreamId with string value.
        /// </summary>
        public StreamId(string id)
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
        /// <returns>Identifier of stream.</returns>
        /// <exception cref="ArgumentNullException">String is null.</exception>
        public static StreamId Parse(string serializedId)
        {
            if (serializedId == null)
            {
                Thrown.ArgumentNullException(nameof(serializedId));
            }

            return new StreamId(serializedId);
        }

        /// <summary>
        /// Parse from string without throwing exception.
        /// </summary>
        /// <param name="serializedId">String value.</param>
        /// <param name="id">Parsed StreamId.</param>
        /// <returns>True if parsed successfully, otherwise false.</returns>
        public static bool TryParse(string serializedId, out StreamId id)
        {
            if (!string.IsNullOrWhiteSpace(serializedId))
            {
                id = StreamId.Parse(serializedId);
                return true;
            }

            id = default(StreamId);
            return false;
        }

        /// <summary>
        /// Get internal id.
        /// </summary>
        public string Id => _id;

        public override string ToString()
        {
            return _id;
        }

        public bool Equals(StreamId other) { return _id == other._id; }

        public override bool Equals(object other)
        {
            if ((other == null) || !(other is StreamId)) return false;

            return this.Equals((StreamId)other);
        }

        public override int GetHashCode() { return _id.GetHashCode(); }

        public static bool operator ==(StreamId a, StreamId b) { return a._id == b._id; }
        public static bool operator !=(StreamId a, StreamId b) { return a._id != b._id; }

#region Parsing overloads
        public static StreamId FromIdentity(IIdentity identity)
        {
            return StreamId.Parse(identity.ToString());
        }

        public static StreamId FromIdentity(CommandId identity)
        {
            return StreamId.Parse(identity.ToString());
        }

        public static StreamId FromIdentity(CommandSequenceId identity)
        {
            return StreamId.Parse(identity.ToString());
        }

        public static StreamId FromIdentity(EventId identity)
        {
            return StreamId.Parse(identity.ToString());
        }

        public static StreamId FromIdentity(PrincipalId identity)
        {
            return StreamId.Parse(identity.ToString());
        }

        public static StreamId FromIdentity(TenantId identity)
        {
            return StreamId.Parse(identity.ToString());
        }

        public static StreamId FromIdentity(TypeMappingId identity)
        {
            return StreamId.Parse(identity.ToString());
        }
#endregion
    }
}
