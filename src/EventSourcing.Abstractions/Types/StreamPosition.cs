using System;
using System.Runtime.InteropServices;

namespace EventSourcing.Abstractions.Types
{
    /// <summary>
    /// Position inside a stream.
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public readonly struct StreamPosition : IEquatable<StreamPosition>, IEquatable<AggregateVersion>
    {
        private readonly long _version;

        public StreamPosition(long version)
        {
            _version = version;
        }

        /// <summary>
        /// Move forward.
        /// </summary>
        /// <param name="distance">Movement distance.</param>
        public StreamPosition Ahead(long distance)
        {
            return new StreamPosition(_version + distance);
        }

        /// <summary>
        /// Move backward.
        /// </summary>
        /// <param name="distance">Movement distance.</param>
        public StreamPosition Backward(long distance)
        {
            return new StreamPosition(_version - distance);
        }

        /// <summary>
        /// Return difference between current position and another position.
        /// </summary>
        /// <param name="streamPosition">Position to compare.</param>
        public long Difference(StreamPosition streamPosition)
        {
            return Math.Abs(streamPosition._version - _version);
        }

        /// <summary>
        /// Position from the beginning of a stream.
        /// </summary>
        public static StreamPosition Begin => new StreamPosition(0);

        /// <summary>
        /// Position from the end of a stream.
        /// </summary>
        public static StreamPosition End => new StreamPosition(long.MaxValue);
        
        public bool Equals(StreamPosition other)
        {
            return _version == other._version;
        }

        public bool Equals(AggregateVersion other)
        {
            return _version == (long)other;
        }

        public override bool Equals(object? obj)
        {
            return obj is StreamPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _version.GetHashCode();
        }

        public override string ToString()
        {
            return _version.ToString();
        }

        public static bool operator ==(StreamPosition left, StreamPosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StreamPosition left, StreamPosition right)
        {
            return !left.Equals(right);
        }
        
        public static bool operator ==(StreamPosition left, AggregateVersion right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StreamPosition left, AggregateVersion right)
        {
            return !left.Equals(right);
        }

        public static StreamPosition operator +(StreamPosition current, StreamPosition other)
        {
            return current.Ahead(other._version);
        }

        public static StreamPosition operator -(StreamPosition current, StreamPosition other)
        {
            return current.Backward(other._version);
        }

        public static implicit operator StreamPosition(long value)
        {
            return new StreamPosition(value);
        }

        public static implicit operator StreamPosition(int value)
        {
            return new StreamPosition(value);
        }

        public static implicit operator long(StreamPosition value)
        {
            return value._version;
        }

        public static implicit operator AggregateVersion(StreamPosition value)
        {
            return new AggregateVersion(value._version);
        }
    }
}