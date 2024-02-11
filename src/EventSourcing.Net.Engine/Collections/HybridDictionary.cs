namespace EventSourcing.Net.Engine.Collections;

using System;
using System.Collections;
using System.Collections.Generic;

public sealed class HybridDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    where TKey : notnull
{
    private const int THRESHOLD = 8;

    private readonly IEqualityComparer<TKey>? _comparer;

    private LinkedDictionary<TKey, TValue>? _list;
    private Dictionary<TKey, TValue>? _dictionary;

    private int _count = 0;

    public HybridDictionary() : this(null)
    {
    }

    public HybridDictionary(IEqualityComparer<TKey>? comparer)
    {
        _comparer = comparer;
        _list = new LinkedDictionary<TKey, TValue>(comparer);
    }

    public HybridDictionary(int capacity, IEqualityComparer<TKey>? comparer)
    {
        _comparer = comparer;
        if (capacity > THRESHOLD)
        {
            _dictionary = new Dictionary<TKey, TValue>(capacity, comparer);
        }
        else
        {
            _list = new LinkedDictionary<TKey, TValue>(comparer);
        }
    }

    public HybridDictionary(int capacity) : this(capacity, null)
    {
    }

    /// <summary>Returns an enumerator that iterates through the collection.</summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        if (_list != null)
        {
            return _list.GetEnumerator();
        }

        IDictionary<TKey, TValue> dictionary = _dictionary;
        return dictionary.GetEnumerator();
    }

    /// <summary>Returns an enumerator that iterates through a collection.</summary>
    /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
    /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
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
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (_list != null)
        {
            if (_count < THRESHOLD)
            {
                _list.Add(key, value);
                _count++;
                return;
            }

            Swap();
        }
        else if (_dictionary != null)
        {
            _dictionary.Add(key, value);
        }
        else
        {
            _list = new(_comparer);
            _list.Add(key, value);
        }

        _count++;
    }

    /// <summary>Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
    public void Clear()
    {
        if (_list != null)
        {
            _list.Clear();
        }

        if (_dictionary != null)
        {
            _dictionary.Clear();
        }

        _count = 0;
    }

    /// <summary>Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.</summary>
    /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, <see langword="false" />.</returns>
    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        if (_list != null)
        {
            return _list.Contains(item);
        }

        if (_dictionary != null)
        {
            IDictionary<TKey, TValue> dictionary = _dictionary;
            return dictionary.Contains(item);
        }

        return false;
    }

    /// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
    /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="array" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="arrayIndex" /> is less than 0.</exception>
    /// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (_list != null)
        {
            _list.CopyTo(array, arrayIndex);
        }
        else if (_dictionary != null)
        {
            IDictionary<TKey, TValue> dictionary = _dictionary;
            dictionary.CopyTo(array, arrayIndex);
        }
    }

    /// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
    /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
    /// <returns>
    /// <see langword="true" /> if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, <see langword="false" />. This method also returns <see langword="false" /> if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        if (_list != null && _list.Remove(item))
        {
            _count--;
            return true;
        }

        if (_dictionary != null && ((IDictionary<TKey, TValue>)_dictionary).Remove(item))
        {
            _count--;
            return true;
        }

        return false;
    }

    /// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
    /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
    public int Count => _count;

    /// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
    /// <returns>
    /// <see langword="true" /> if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, <see langword="false" />.</returns>
    public bool IsReadOnly => false;

    /// <summary>Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.</summary>
    /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="key" /> is <see langword="null" />.</exception>
    /// <returns>
    /// <see langword="true" /> if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, <see langword="false" />.</returns>
    public bool ContainsKey(TKey key)
    {
        if (_list != null)
        {
            return _list.ContainsKey(key);
        }

        if (_dictionary != null)
        {
            return _dictionary.ContainsKey(key);
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
        if (_list != null && _list.Remove(key))
        {
            _count--;
            return true;
        }

        if (_dictionary != null && _dictionary.Remove(key))
        {
            _count--;
            return true;
        }

        return false;
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
        if (_list != null)
        {
            return _list.TryGetValue(key, out value);
        }

        return _dictionary.TryGetValue(key, out value);
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
            if (_list != null)
            {
                return _list[key];
            }

            return _dictionary[key];
        }
        set
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (_list != null)
            {
                _list[key] = value;
                _count = _list.Count;
                if (_count > THRESHOLD)
                {
                    Swap();
                }

                return;
            }

            if (_dictionary != null)
            {
                _dictionary[key] = value;
                _count = _dictionary.Count;
            }
            else
            {
                _list = new(_comparer);
                _list.Add(key, value);
                _count = 1;
            }
        }
    }

    /// <summary>Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
    /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
    public ICollection<TKey> Keys
    {
        get
        {
            if (_list != null)
            {
                return _list.Keys;
            }

            if (_dictionary != null)
            {
                return _dictionary.Keys;
            }

            return Array.Empty<TKey>();
        }
    }

    /// <summary>Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
    /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
    public ICollection<TValue> Values
    {
        get
        {
            if (_list != null)
            {
                return _list.Values;
            }

            if (_dictionary != null)
            {
                return _dictionary.Values;
            }

            return Array.Empty<TValue>();
        }
    }

    private void Swap()
    {
        _dictionary = new Dictionary<TKey, TValue>(THRESHOLD + 2, _comparer);
        _list.CopyTo(_dictionary);
        _list = null;
    }
}