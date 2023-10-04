namespace EventSourcing.Net.Samples.CustomIdTypes
{
    using System.ComponentModel;
    using EventSourcing.Net.Abstractions.Contracts;
    using TypeConverters;

    /// <summary>
    /// Int based identifier.
    /// </summary>
    /// <remarks>Generated from SpeedDating.CodeGeneration.Identities.tt</remarks>
    [TypeConverter(typeof(IntIdConverter))]
    public readonly struct IntId : IEquatable<IntId>
    {
        private readonly int _id;

        /// <summary>
        /// Get the prefix.
        /// </summary>
        public static readonly string Prefix = "int_";

        /// <summary>
        /// Get empty IntId.
        /// </summary>
        public static readonly IntId Empty = new IntId(default(int));

        /// <summary>
        /// Initiate new object.
        /// </summary>
        public IntId(int id)
        {
            _id = id;
        }

        /// <summary>
        /// Parse from string.
        /// </summary>
        /// <param name="serializedId">String value.</param>
        /// <returns>Int based identifier.</returns>
        /// <exception cref="ArgumentException">String cannot be parsed to IntId.</exception>
        public static IntId Parse(string serializedId)
        {
            if (!serializedId.StartsWith(Prefix, StringComparison.Ordinal))
            {
                throw new ArgumentException("Invalid IntId", "serializedId");
            }

            ReadOnlySpan<char> span = serializedId.AsSpan();
            int id = int.Parse(span.Slice(Prefix.Length));

            return new IntId(id);
        }

        /// <summary>
        /// Parse from string without throwing exception.
        /// </summary>
        /// <param name="serializedId">String value.</param>
        /// <param name="id">Parsed IntId.</param>
        /// <returns>True if parsed successfully, otherwise false.</returns>
        public static bool TryParse(string serializedId, out IntId id)
        {
            int value;

            if (!string.IsNullOrWhiteSpace(serializedId) && serializedId.StartsWith(Prefix, StringComparison.Ordinal))
            {
                ReadOnlySpan<char> span = serializedId.AsSpan().Slice(Prefix.Length);
                if (int.TryParse(span, out value))
                {
                    id = new IntId(value);
                    return true;
                }
            }

            id = default(IntId);
            return false;
        }

        /// <summary>
        /// Get internal id.
        /// </summary>
        public int Id => _id;

        public static implicit operator int(IntId id)
        {
            return id._id;
        }

        public static implicit operator IntId(int id)
        {
            return new IntId(id);
        }

        public override string ToString()
        {
            return string.Concat(Prefix, _id.ToString());
        }

        public bool Equals(IntId other) { return _id == other._id; }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is IntId)) 
            {
                return false;
            }

            return this.Equals((IntId)obj);
        }

        public override int GetHashCode() { return _id.GetHashCode(); }

        public static bool operator ==(IntId a, IntId b) { return a._id == b._id; }
        public static bool operator !=(IntId a, IntId b) { return a._id != b._id; }
    }
}

