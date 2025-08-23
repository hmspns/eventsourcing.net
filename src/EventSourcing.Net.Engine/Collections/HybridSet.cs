namespace EventSourcing.Net.Engine.Collections;

using System;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// A hybrid collection that combines LinkedList and HashSet to optimize memory usage and performance.
/// Uses LinkedList for small collections and automatically switches to HashSet when size exceeds threshold.
/// </summary>
/// <typeparam name="T">The type of elements in the set.</typeparam>
public sealed class HybridSet<T> : ISet<T>, IReadOnlySet<T>, ICollection<T>, IReadOnlyCollection<T> where T : notnull
{
    private const int THRESHOLD = 8;

    private readonly IEqualityComparer<T>? _comparer;

    private LinkedSet<T>? _list;

    private HashSet<T>? _hash;

    private int _count;

    /// <summary>
    /// Initializes a new instance of the HybridSet class that is empty.
    /// </summary>
    public HybridSet() : this(null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the HybridSet class that uses the specified equality comparer.
    /// </summary>
    /// <param name="comparer">The IEqualityComparer implementation to use when comparing values.</param>
    public HybridSet(IEqualityComparer<T>? comparer)
    {
        _comparer = comparer;
        _list = new LinkedSet<T>(comparer);
        _hash = null;
        _count = 0;
    }

    /// <summary>
    /// Initializes a new instance of the HybridSet class with specified initial capacity.
    /// </summary>
    /// <param name="capacity">The initial number of elements that the set can contain.</param>
    public HybridSet(int capacity) : this(capacity, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the HybridSet class with specified initial capacity and equality comparer.
    /// </summary>
    /// <param name="capacity">The initial number of elements that the set can contain.</param>
    /// <param name="comparer">The IEqualityComparer implementation to use when comparing values.</param>
    public HybridSet(int capacity, IEqualityComparer<T>? comparer)
    {
        _comparer = comparer;
        if (capacity > THRESHOLD)
        {
            _hash = comparer == null ? new HashSet<T>() : new HashSet<T>(comparer);
            _hash.EnsureCapacity(capacity);
            _list = null;
        }
        else
        {
            _list = new LinkedSet<T>(comparer);
            _hash = null;
        }
        _count = 0;
    }

    /// <summary>
    /// Gets the number of elements contained in the set.
    /// </summary>
    public int Count => _count;

    /// <summary>
    /// Gets a value indicating whether the set is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Adds an item to the set.
    /// </summary>
    /// <param name="item">The item to add to the set.</param>
    /// <returns>True if the item was added to the set; false if the item already exists.</returns>
    public bool Add(T item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (_list != null)
        {
            if (_count < THRESHOLD)
            {
                if (_list.Add(item))
                {
                    _count++;
                    return true;
                }
                return false;
            }

            SwapToHash();
        }

        if (_hash != null)
        {
            bool added = _hash.Add(item);
            if (added)
            {
                _count++;
            }
            return added;
        }

        // Initialize list if both structures are null
        _list = new LinkedSet<T>(_comparer);
        bool wasAdded = _list.Add(item);
        if (wasAdded)
        {
            _count = 1;
        }
        return wasAdded;
    }

    void ICollection<T>.Add(T item) => Add(item);

    /// <summary>
    /// Removes all items from the set.
    /// </summary>
    public void Clear()
    {
        _list?.Clear();
        _hash?.Clear();
        _count = 0;
    }

    /// <summary>
    /// Determines whether the set contains a specific item.
    /// </summary>
    /// <param name="item">The item to locate in the set.</param>
    /// <returns>True if the item is found in the set; otherwise, false.</returns>
    public bool Contains(T item)
    {
        if (_list != null)
        {
            return _list.Contains(item);
        }

        return _hash != null && _hash.Contains(item);
    }

    /// <summary>
    /// Copies the elements of the set to an array, starting at a particular array index.
    /// </summary>
    /// <param name="array">The one-dimensional array that is the destination of the elements.</param>
    /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        if (_list != null)
        {
            _list.CopyTo(array, arrayIndex);
            return;
        }

        _hash?.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Removes the specified item from the set.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was successfully found and removed; otherwise, false.</returns>
    public bool Remove(T item)
    {
        if (_list != null)
        {
            if (_list.Remove(item))
            {
                _count--;
                return true;
            }
            return false;
        }

        if (_hash != null && _hash.Remove(item))
        {
            _count--;
            return true;
        }

        return false;
    }

    public Enumerator GetEnumerator() => new Enumerator(this);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // ISet<T> implementation - forces switch to HashSet with delegation
    /// <summary>
    /// Modifies the current set to contain all elements that are present in itself, the specified collection, or both.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    public void UnionWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureHash();
        _hash!.UnionWith(other);
        _count = _hash.Count;
    }

    /// <summary>
    /// Modifies the current set to contain only elements that are present in both itself and the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    public void IntersectWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureHash();
        _hash!.IntersectWith(other);
        _count = _hash.Count;
    }

    /// <summary>
    /// Removes all elements in the specified collection from the current set.
    /// </summary>
    /// <param name="other">The collection of items to remove from the set.</param>
    public void ExceptWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureHash();
        _hash!.ExceptWith(other);
        _count = _hash.Count;
    }

    /// <summary>
    /// Modifies the current set to contain only elements that are present either in itself or in the specified collection, but not both.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureHash();
        _hash!.SymmetricExceptWith(other);
        _count = _hash.Count;
    }

    /// <summary>
    /// Determines whether the current set is a subset of the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns>True if the current set is a subset of other; otherwise, false.</returns>
    public bool IsSubsetOf(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureHash();
        return _hash!.IsSubsetOf(other);
    }

    /// <summary>
    /// Determines whether the current set is a superset of the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns>True if the current set is a superset of other; otherwise, false.</returns>
    public bool IsSupersetOf(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureHash();
        return _hash!.IsSupersetOf(other);
    }

    /// <summary>
    /// Determines whether the current set is a proper superset of the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns>True if the current set is a proper superset of other; otherwise, false.</returns>
    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureHash();
        return _hash!.IsProperSupersetOf(other);
    }

    /// <summary>
    /// Determines whether the current set is a proper subset of the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns>True if the current set is a proper subset of other; otherwise, false.</returns>
    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureHash();
        return _hash!.IsProperSubsetOf(other);
    }

    /// <summary>
    /// Determines whether the current set overlaps with the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns>True if the current set and other share at least one common element; otherwise, false.</returns>
    public bool Overlaps(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureHash();
        return _hash!.Overlaps(other);
    }

    /// <summary>
    /// Determines whether the current set and the specified collection contain the same elements.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns>True if the current set is equal to other; otherwise, false.</returns>
    public bool SetEquals(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureHash();
        return _hash!.SetEquals(other);
    }

    private void EnsureHash()
    {
        if (_hash != null)
        {
            return;
        }

        SwapToHash();
    }

    private void SwapToHash()
    {
        _hash = _comparer == null ? new HashSet<T>() : new HashSet<T>(_comparer);
        if (_list != null && _list.Count > 0)
        {
            _list.CopyTo(_hash);
        }
        _list = null;
        _count = _hash.Count;
    }

    public struct Enumerator : IEnumerator<T>
    {
        private HashSet<T>.Enumerator _hashEnumerator;
        private LinkedSet<T>.Enumerator _listEnumerator;
        private readonly bool _useHash;

        internal Enumerator(HybridSet<T> set)
        {
            if (set._hash != null)
            {
                _useHash = true;
                _hashEnumerator = set._hash.GetEnumerator();
                _listEnumerator = default;
            }
            else
            {
                _useHash = false;
                _listEnumerator = set._list!.GetEnumerator();
                _hashEnumerator = default;
            }
        }

        public bool MoveNext()
        {
            return _useHash ? _hashEnumerator.MoveNext() : _listEnumerator.MoveNext();
        }

        public void Reset()
        {
            if (_useHash)
            {
                IEnumerator enumerator = _hashEnumerator;
                enumerator.Reset();
            }
            else
            {
                _listEnumerator.Reset();
            }
        }

        public T Current => _useHash ? _hashEnumerator.Current : _listEnumerator.Current;

        object? IEnumerator.Current => Current;

        public void Dispose()
        {
            if (_useHash)
            {
                _hashEnumerator.Dispose();
            }
            else
            {
                _listEnumerator.Dispose();
            }
        }
    }
}
