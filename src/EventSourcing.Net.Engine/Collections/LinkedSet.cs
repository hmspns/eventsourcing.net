namespace EventSourcing.Net.Engine.Collections;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Exceptions;

/// <summary>
/// A linked list-based implementation of a set data structure.
/// Provides constant-time insertion and removal operations at the expense of linear-time lookups.
/// This implementation is optimized for small sets with frequent modifications.
/// </summary>
/// <typeparam name="T">The type of elements in the set. Must be non-null.</typeparam>
internal sealed class LinkedSet<T> : ISet<T>, IReadOnlySet<T> where T : notnull
{
    private Node? _head;
    private byte _count;
    private readonly IEqualityComparer<T>? _comparer;

    /// <summary>
    /// Initializes a new instance of the LinkedSet class with the specified equality comparer.
    /// </summary>
    /// <param name="comparer">The equality comparer to use for comparing elements, or null to use the default comparer.</param>
    internal LinkedSet(IEqualityComparer<T>? comparer)
    {
        _comparer = comparer;
    }

    /// <summary>
    /// Copies all elements from this set to the specified collection.
    /// </summary>
    /// <param name="destination">The collection to copy elements to.</param>
    internal void CopyTo(ICollection<T> destination)
    {
        for (Node? node = _head; node != null; node = node.next)
        {
            destination.Add(node.value);
        }
    }

    /// <summary>
    /// Gets the number of elements in the set.
    /// </summary>
    public int Count => _count;

    /// <summary>
    /// Gets a value indicating whether the set is read-only.
    /// Always returns false for this implementation.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Adds the specified item to the set if it's not already present.
    /// </summary>
    /// <param name="item">The item to add to the set.</param>
    /// <returns>true if the item was added; false if it already existed.</returns>
    public bool Add(T item)
    {
        ArgumentNullException.ThrowIfNull(item);

        Node? last = null;

        if (_comparer == null)
        {
            for (Node? node = _head; node != null; node = node.next)
            {
                if (node.value.Equals(item))
                {
                    return false;
                }
                last = node;
            }
        }
        else
        {
            for (Node? node = _head; node != null; node = node.next)
            {
                if (_comparer.Equals(node.value, item))
                {
                    return false;
                }
                last = node;
            }
        }

        Node newNode = new Node(item);

        if (last != null)
        {
            last.next = newNode;
        }
        else
        {
            _head = newNode;
        }

        _count++;
        return true;
    }

    void ICollection<T>.Add(T item) => Add(item);

    public void Clear()
    {
        _head = null;
        _count = 0;
    }

    public bool Contains(T item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (_comparer == null)
        {
            for (Node? node = _head; node != null; node = node.next)
            {
                if (node.value.Equals(item))
                {
                    return true;
                }
            }
        }
        else
        {
            for (Node? node = _head; node != null; node = node.next)
            {
                if (_comparer.Equals(node.value, item))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);

        if (arrayIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        }

        if (array.Length - arrayIndex < _count)
        {
            throw new ArgumentException("Not enough space in array");
        }

        for (Node? node = _head; node != null; node = node.next)
        {
            array[arrayIndex++] = node.value;
        }
    }

    public bool Remove(T item)
    {
        ArgumentNullException.ThrowIfNull(item);

        Node? last = null;
        Node? node;
        for (node = _head; node != null; node = node.next)
        {
            if ((_comparer == null) ? node.value.Equals(item) : _comparer.Equals(node.value, item))
            {
                break;
            }
            last = node;
        }

        if (node == null)
        {
            return false;
        }

        if (node == _head)
        {
            _head = node.next;
        }
        else
        {
            last!.next = node.next;
        }

        _count--;
        return true;
    }

    public Enumerator GetEnumerator() => new Enumerator(this);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    /// <summary>
    /// Modifies the current set to contain all elements that are present in either the current set or the specified collection.
    /// </summary>
    /// <param name="other">The collection of items to add to the set.</param>
    public void UnionWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        foreach (T item in other)
        {
            Add(item);
        }
    }

    /// <summary>
    /// Modifies the current set to contain only elements that are present in both the current set and the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    public void IntersectWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        HashSet<T> set = _comparer == null ? new HashSet<T>(other) : new HashSet<T>(other, _comparer);
        Node? prev = null;
        Node? node = _head;
        while (node != null)
        {
            bool keep = set.Contains(node.value);
            if (!keep)
            {
                if (node == _head)
                {
                    _head = node.next;
                    node = _head;
                }
                else
                {
                    prev!.next = node.next;
                    node = prev.next;
                }
                _count--;
            }
            else
            {
                prev = node;
                node = node.next;
            }
        }
    }

    /// <summary>
    /// Removes all elements in the specified collection from the current set.
    /// </summary>
    /// <param name="other">The collection of items to remove from the set.</param>
    public void ExceptWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        HashSet<T> set = _comparer == null ? new HashSet<T>(other) : new HashSet<T>(other, _comparer);
        Node? prev = null;
        Node? node = _head;
        while (node != null)
        {
            bool remove = set.Contains(node.value);
            if (remove)
            {
                if (node == _head)
                {
                    _head = node.next;
                    node = _head;
                }
                else
                {
                    prev!.next = node.next;
                    node = prev.next;
                }
                _count--;
            }
            else
            {
                prev = node;
                node = node.next;
            }
        }
    }

    /// <summary>
    /// Modifies the current set to contain only elements that are present either in the current set or in the specified collection, but not both.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        foreach (T item in other)
        {
            if (!Remove(item))
            {
                Add(item);
            }
        }
    }

    /// <summary>
    /// Determines whether the current set is a subset of the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns>true if the current set is a subset of the specified collection; otherwise, false.</returns>
    public bool IsSubsetOf(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        HashSet<T> set = _comparer == null ? new HashSet<T>(other) : new HashSet<T>(other, _comparer);
        for (Node? node = _head; node != null; node = node.next)
        {
            if (!set.Contains(node.value))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Determines whether the current set is a superset of the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns>true if the current set is a superset of the specified collection; otherwise, false.</returns>
    public bool IsSupersetOf(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        foreach (T item in other)
        {
            if (!Contains(item))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Determines whether the current set is a proper superset of the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns>true if the current set is a proper superset of the specified collection; otherwise, false.</returns>
    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        HashSet<T> set = _comparer == null ? new HashSet<T>(other) : new HashSet<T>(other, _comparer);
        if (Count <= set.Count) return false;
        return IsSupersetOf(set);
    }

    /// <summary>
    /// Determines whether the current set is a proper subset of the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns>true if the current set is a proper subset of the specified collection; otherwise, false.</returns>
    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        HashSet<T> set = _comparer == null ? new HashSet<T>(other) : new HashSet<T>(other, _comparer);
        if (Count >= set.Count) return false;
        return IsSubsetOf(set);
    }

    /// <summary>
    /// Determines whether the current set overlaps with the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns>true if the current set and the specified collection share at least one common element; otherwise, false.</returns>
    public bool Overlaps(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        foreach (T item in other)
        {
            if (Contains(item))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Determines whether the current set and the specified collection contain the same elements.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns>true if the current set is equal to the specified collection; otherwise, false.</returns>
    public bool SetEquals(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        HashSet<T> set = _comparer == null ? new HashSet<T>(other) : new HashSet<T>(other, _comparer);
        if (set.Count != Count)
        {
            return false;
        }

        for (Node? node = _head; node != null; node = node.next)
        {
            if (!set.Contains(node.value))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Enumerator for traversing the LinkedSet elements.
    /// Provides a forward-only cursor through the set.
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct Enumerator : IEnumerator<T>
    {
        private LinkedSet<T>? _set;
        private Node? _current;
        private bool _isStart;

        internal Enumerator(LinkedSet<T> set)
        {
            _set = set;
            _isStart = true;
            _current = null;
        }

        public bool MoveNext()
        {
            if (_isStart)
            {
                _current = _set!._head;
                _isStart = false;
            }
            else if (_current != null)
            {
                _current = _current.next;
            }

            return _current != null;
        }

        public void Reset()
        {
            _isStart = true;
            _current = null;
        }

        public T Current
        {
            get
            {
                if (_current == null)
                {
                    Thrown.InvalidOperationException("MoveNext method should be called before");
                }

                return _current.value;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                if (_current == null)
                {
                    Thrown.InvalidOperationException("MoveNext method should be called before");
                }

                return _current.value!;
            }
        }

        public void Dispose()
        {
            _current = null;
            _set = null;
        }
    }

    /// <summary>
    /// Represents a node in the linked list structure.
    /// Contains the value and a reference to the next node.
    /// </summary>
    private sealed class Node
    {
        /// <summary>
        /// Initializes a new node with the specified value.
        /// </summary>
        /// <param name="value">The value to store in the node.</param>
        internal Node(T value) => this.value = value;

        /// <summary>
        /// The value stored in this node.
        /// </summary>
        internal readonly T value;

        /// <summary>
        /// Reference to the next node in the linked list, or null if this is the last node.
        /// </summary>
        internal Node? next;
    }
}
