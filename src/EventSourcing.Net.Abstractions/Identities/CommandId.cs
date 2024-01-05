// <auto-generated />
using System;
using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Net.Abstractions.Identities
{
    /// <summary>
    /// Command identifier.
    /// </summary>
    /// <remarks>Generated from EventSourcing.Net.CodeGeneration.Identities.tt</remarks>
    public readonly struct CommandId : IIdentity, IEquatable<CommandId>
    {
        private readonly Guid _id;

        /// <summary>
        /// Get the prefix.
        /// </summary>
        public static readonly string Prefix = "command_";

        /// <summary>
        /// Get empty CommandId.
        /// </summary>
        public static readonly CommandId Empty = new CommandId(Guid.Empty);

        /// <summary>
        /// Create CommandId with random value.
        /// </summary>
        public static CommandId New()
        {
            return new CommandId(Guid.NewGuid());
        }

        /// <summary>
        /// Initiate new object.
        /// </summary>
        public CommandId(Guid id)
        {
            _id = id;
        }

        /// <summary>
        /// Parse from string.
        /// </summary>
        /// <param name="serializedId">String value.</param>
        /// <returns>Command identifier.</returns>
        /// <exception cref="ArgumentException">String cannot be parsed to CommandId.</exception>
        public static CommandId Parse(string serializedId)
        {
            if (serializedId?.StartsWith(Prefix, StringComparison.Ordinal) != true)
            {
                Thrown.ArgumentException("CommandId shouldn't be null and should starts with prefix " + Prefix, nameof(serializedId));
            }

            ReadOnlySpan<char> span = serializedId.AsSpan();
            Guid id = Guid.Parse(span.Slice(Prefix.Length));

            return new CommandId(id);
        }

        /// <summary>
        /// Parse from string without throwing exception.
        /// </summary>
        /// <param name="serializedId">String value.</param>
        /// <param name="id">Parsed CommandId.</param>
        /// <returns>True if parsed successfully, otherwise false.</returns>
        public static bool TryParse(string serializedId, out CommandId id)
        {
            Guid guid;

            if (!string.IsNullOrWhiteSpace(serializedId) && serializedId.StartsWith(Prefix, StringComparison.Ordinal))
            {
                ReadOnlySpan<char> span = serializedId.AsSpan().Slice(Prefix.Length);
                if (Guid.TryParse(span, out guid))
                {
                    id = new CommandId(guid);
                    return true;
                }
            }

            id = default(CommandId);
            return false;
        }

        /// <summary>
        /// Get internal id.
        /// </summary>
        public Guid Id => _id;

        public static implicit operator Guid(CommandId id)
        {
            return id._id;
        }

        public static implicit operator CommandId(Guid id)
        {
            return new CommandId(id);
        }

        public override string ToString()
        {
            return string.Concat(Prefix, _id.ToString());
        }

        public bool Equals(CommandId other) { return _id == other._id; }

        public override bool Equals(object other)
        {
            if ((other == null) || !(other is CommandId)) 
            {
                return false;
            }

            return this.Equals((CommandId)other);
        }

        public override int GetHashCode() { return _id.GetHashCode(); }

        public static bool operator ==(CommandId a, CommandId b) { return a._id == b._id; }
        public static bool operator !=(CommandId a, CommandId b) { return a._id != b._id; }
    }
}
