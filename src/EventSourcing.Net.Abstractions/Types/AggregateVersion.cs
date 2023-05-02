using System;

namespace EventSourcing.Net.Abstractions.Types;

/// <summary>
/// Version of the aggregate.
/// </summary>
public readonly struct AggregateVersion : IEquatable<AggregateVersion>, IEquatable<StreamPosition>, IComparable<AggregateVersion>, IComparable
{
    private readonly long _version;

    public AggregateVersion(long version)
    {
        _version = version;
    }

    public AggregateVersion(int version)
    {
        _version = version;
    }
        
    /// <summary>
    /// Version of aggregate without events.
    /// </summary>
    public static AggregateVersion NotCreated => new AggregateVersion(0);

    public static implicit operator AggregateVersion(long version)
    {
        return new AggregateVersion(version);
    }

    public static implicit operator AggregateVersion(int version)
    {
        return new AggregateVersion(version);
    }

    public static implicit operator long(AggregateVersion version)
    {
        return version._version;
    }
        
    public static implicit operator StreamPosition(AggregateVersion value)
    {
        return new StreamPosition(value._version);
    }
        
    public override string ToString()
    {
        return _version.ToString();
    }

    public int CompareTo(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return 1;
        }

        if (obj is AggregateVersion other)
        {
            return CompareTo(other);
        }
            
        throw new ArgumentException($"Object must be of type {nameof(AggregateVersion)}");
    }

    public int CompareTo(AggregateVersion other)
    {
        return _version.CompareTo(other._version);
    }
        
    public bool Equals(AggregateVersion other)
    {
        return _version == other._version;
    }
        
    public bool Equals(StreamPosition other)
    {
        return _version == (long)other;
    }

    public override bool Equals(object? obj)
    {
        return obj is AggregateVersion other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _version.GetHashCode();
    }

    public static bool operator ==(AggregateVersion left, AggregateVersion right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AggregateVersion left, AggregateVersion right)
    {
        return !left.Equals(right);
    }
        
    public static bool operator ==(AggregateVersion left, StreamPosition right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AggregateVersion left, StreamPosition right)
    {
        return !left.Equals(right);
    }

    public static bool operator >(AggregateVersion left, AggregateVersion right)
    {
        return left._version > right._version;
    }

    public static bool operator <(AggregateVersion left, AggregateVersion right)
    {
        return left._version < right._version;
    }
        
    public static bool operator >=(AggregateVersion left, AggregateVersion right)
    {
        return left._version >= right._version;
    }

    public static bool operator <=(AggregateVersion left, AggregateVersion right)
    {
        return left._version <= right._version;
    }
}