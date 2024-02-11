namespace EventSourcing.Net.Engine.Collections;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Exceptions;

internal sealed class LinkedDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull
{
    private Node? _head;
    private byte _count;
    private readonly IEqualityComparer<TKey>? _comparer;

    internal LinkedDictionary(IEqualityComparer<TKey>? comparer)
    {
        _comparer = comparer;
    }

    internal void CopyTo(IDictionary<TKey, TValue> dictionary)
    {
        for (Node? node = _head; node != null; node = node.next)
        {
            dictionary.Add(node.key, node.value);    
        }
    }

    /// <summary>Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
    /// <param name="key">The object to use as the key of the element to add.</param>
    /// <param name="value">The object to use as the value of the element to add.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="key" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</exception>
    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.</exception>
    public void Add(TKey key, TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);

        Node? last = null;

        if (_comparer == null)
        {
            for (Node? node = _head; node != null; node = node.next)
            {
                TKey currentKey = node.key;
                if (currentKey.Equals(key))
                {
                    throw new ArgumentException("Added duplicate key", nameof(key));
                }

                last = node;
            }
        }
        else
        {
            for (Node? node = _head; node != null; node = node.next)
            {
                TKey currentKey = node.key;
                if (_comparer.Equals(currentKey, key))
                {
                    throw new ArgumentException("Added duplicate key", nameof(key));
                }

                last = node;
            }
        }
        
        Node newNode = new Node(key, value);

        if (last != null)
        {
            last.next = newNode;
        }
        else
        {
            _head = newNode;
        }

        _count++;
    }

    /// <summary>Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.</summary>
    /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="key" /> is <see langword="null" />.</exception>
    /// <returns>
    /// <see langword="true" /> if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, <see langword="false" />.</returns>
    public bool ContainsKey(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        for (Node? node = _head; node != null; node = node.next)
        {
            TKey oldKey = node.key;
            if (_comparer == null ? oldKey.Equals(key) : _comparer.Equals(oldKey, key))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="key" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.</exception>
    /// <returns>
    /// <see langword="true" /> if the element is successfully removed; otherwise, <see langword="false" />.  This method also returns <see langword="false" /> if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
    public bool Remove(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        Node? last = null;
        Node? node;
        for (node = _head; node != null; node = node.next)
        {
            TKey oldKey = node.key;
            if ((_comparer == null) ? oldKey.Equals(key) : _comparer.Equals(oldKey, key))
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

    /// <summary>Gets the value associated with the specified key.</summary>
    /// <param name="key">The key whose value to get.</param>
    /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="key" /> is <see langword="null" />.</exception>
    /// <returns>
    /// <see langword="true" /> if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, <see langword="false" />.</returns>
    public bool TryGetValue(TKey key, out TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (_comparer == null)
        {
            for (Node? node = _head; node != null; node = node.next)
            {
                if (key.Equals(node.key))
                {
                    value = node.value;
                    return true;
                }
            }
        }
        else
        {
            for (Node? node = _head; node != null; node = node.next)
            {
                if (_comparer.Equals(node.key, key))
                {
                    value = node.value;
                    return true;
                }
            }
        }

        value = default!;
        return false;
    }

    /// <summary>Gets or sets the element with the specified key.</summary>
    /// <param name="key">The key of the element to get or set.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="key" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key" /> is not found.</exception>
    /// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.</exception>
    /// <returns>The element with the specified key.</returns>
    public TValue this[TKey key]
    {
        get
        {
            if (!TryGetValue(key, out var value))
            {
                throw new KeyNotFoundException();
            }

            return value;
        }
        set
        {
            ArgumentNullException.ThrowIfNull(key);
            Node? last = null;

            if (_comparer == null)
            {
                for (Node? node = _head; node != null; node = node.next)
                {
                    if (node.key.Equals(key))
                    {
                        node.value = value;
                        return;
                    }

                    last = node;
                }
            }
            else
            {
                for (Node? node = _head; node != null; node = node.next)
                {
                    if (_comparer.Equals(node.key, key))
                    {
                        node.value = value;
                        return;
                    }

                    last = node;
                }
            }


            Node newNode = new Node(key, value);

            if (last != null)
            {
                last.next = newNode;
            }
            else
            {
                _head = newNode;
            }

            _count++;
        }
    }

    /// <summary>Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
    /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
    public ICollection<TKey> Keys
    {
        get
        {
            List<TKey> keys = new List<TKey>(_count);
            for (Node? node = _head; node != null; node = node.next)
            {
                keys.Add(node.key);
            }

            return keys;
        }
    }

    /// <summary>Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
    /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
    public ICollection<TValue> Values
    {
        get
        {
            List<TValue> values = new List<TValue>(_count);
            for (Node? node = _head; node != null; node = node.next)
            {
                values.Add(node.value);
            }

            return values;
        }
    }

    /// <summary>Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
    /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    /// <summary>Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
    public void Clear()
    {
        _count = 0;
        _head = null;
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        if (!TryGetValue(item.Key, out TValue value))
        {
            return false;
        }

        return EqualityComparer<TValue>.Default.Equals(item.Value, value);
    }

    /// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
    /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="array" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="arrayIndex" /> is less than 0.</exception>
    /// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
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
            array[arrayIndex++] = new KeyValuePair<TKey, TValue>(node.key, node.value);
        }
    }

    /// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
    /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
    /// <returns>
    /// <see langword="true" /> if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, <see langword="false" />. This method also returns <see langword="false" /> if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        TKey key = item.Key;
        ArgumentNullException.ThrowIfNull(key);

        Node? last = null;
        Node? node;
        for (node = _head; node != null; node = node.next)
        {
            TKey oldKey = node.key;
            if ((_comparer == null) ? oldKey.Equals(key) : _comparer.Equals(oldKey, key))
            {
                break;
            }

            last = node;
        }

        if (node == null || !EqualityComparer<TValue>.Default.Equals(item.Value, node.value))
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

    /// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
    /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
    public int Count => _count;

    /// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
    /// <returns>
    /// <see langword="true" /> if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, <see langword="false" />.</returns>
    public bool IsReadOnly => true;


    /// <summary>Returns an enumerator that iterates through the collection.</summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return new LinkedDictionaryEnumerator(this);
    }

    /// <summary>Returns an enumerator that iterates through a collection.</summary>
    /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private sealed class Node
    {
        internal Node(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }
        
        public TKey key;

        public TValue value;

        public Node? next;
    }

    [StructLayout(LayoutKind.Auto)]
    private struct LinkedDictionaryEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        private LinkedDictionary<TKey, TValue>? _dictionary;
        private Node? _current;
        private bool _isStart;

        internal LinkedDictionaryEnumerator(LinkedDictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
            _isStart = true;
            _current = null;
        }

        /// <summary>Advances the enumerator to the next element of the collection.</summary>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created.</exception>
        /// <returns>
        /// <see langword="true" /> if the enumerator was successfully advanced to the next element; <see langword="false" /> if the enumerator has passed the end of the collection.</returns>
        public bool MoveNext()
        {
            if (_isStart)
            {
                _current = _dictionary!._head;
                _isStart = false;
            }
            else if (_current != null)
            {
                _current = _current.next;
            }

            return _current != null;
        }

        /// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created.</exception>
        /// <exception cref="T:System.NotSupportedException">The enumerator does not support being reset.</exception>
        public void Reset()
        {
            _isStart = true;
            _current = null;
        }

        KeyValuePair<TKey, TValue> IEnumerator<KeyValuePair<TKey, TValue>>.Current
        {
            get
            {
                if (_current == null)
                {
                    Thrown.InvalidOperationException("MoveNext method should be called before");
                }

                return new KeyValuePair<TKey, TValue>(_current.key, _current.value);
            }
        }

        /// <summary>Gets the element in the collection at the current position of the enumerator.</summary>
        /// <returns>The element in the collection at the current position of the enumerator.</returns>
        public object Current
        {
            get
            {
                if (_current == null)
                {
                    Thrown.InvalidOperationException("MoveNext method should be called before");
                }

                return new KeyValuePair<TKey, TValue>(_current.key, _current.value);
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _current = null;
            _dictionary = null;
        }
    }
}