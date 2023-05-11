/*
 * The MIT License (MIT)
 *
 * Copyright (c) Joel Mueller
 *
 * All rights reserved.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

namespace EventSourcing.Net.Engine.Pooled.Collections;

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;

/// <summary>
/// Implements a variable-size list that uses a pooled array to store the
/// elements. A PooledList has a capacity, which is the allocated length
/// of the internal array. As elements are added to a PooledList, the capacity
/// of the PooledList is automatically increased as required by reallocating the
/// internal array.
/// </summary>
/// <remarks>
/// This class is based on the code for <see cref="List{T}" /> but it supports <see cref="Span{T}" />
/// and uses <see cref="ArrayPool{T}" /> when allocating internal arrays.
/// </remarks>
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(CollectionDebugView<>))]
[Serializable]
public sealed class PooledList<T> : IList<T>, IReadOnlyPooledList<T>, IList, IDisposable, IDeserializationCallback
{
    // internal constant copied from Array.MaxArrayLength
    private const int MAX_ARRAY_LENGTH = 0x7FEFFFFF;
    private const int DEFAULT_CAPACITY = 4;
    private static readonly T[] _emptyArray = Array.Empty<T>();
    private readonly bool _clearOnFree;

    private T[] _items; // Do not rename (binary serialization)

    [NonSerialized] private ArrayPool<T> _pool;
    [NonSerialized] private object? _syncRoot;
    private int _version; // Do not rename (binary serialization)

    /// <summary>
    /// Gets a <see cref="System.Span{T}" /> for the items currently in the collection.
    /// </summary>
    public Span<T> Span => _items.AsSpan(0, Count);

    /// <summary>
    /// Gets and sets the capacity of this list.  The capacity is the size of
    /// the internal array used to hold items.  When set, the internal
    /// Memory of the list is reallocated to the given capacity.
    /// Note that the return value for this property may be larger than the property was set to.
    /// </summary>
    public int Capacity
    {
        get => _items.Length;
        set
        {
            if (value < Count)
            {
                OtherThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value,
                ExceptionResource.ArgumentOutOfRange_SmallCapacity);
            }

            if (value != _items.Length)
            {
                if (value > 0)
                {
                    T[] newItems = _pool.Rent(value);
                    if (Count > 0)
                    {
                        Array.Copy(_items, newItems, Count);
                    }

                    ReturnArray();
                    _items = newItems;
                }
                else
                {
                    ReturnArray();
                    Count = 0;
                }
            }
        }
    }

    /// <summary>
    /// Returns the ClearMode behavior for the collection, denoting whether values are
    /// cleared from internal arrays before returning them to the pool.
    /// </summary>
    public ClearMode ClearMode => _clearOnFree ? ClearMode.Always : ClearMode.Never;

    /// <summary>
    /// Read-only property describing how many elements are in the List.
    /// </summary>
    public int Count { get; private set; }

    bool IList.IsFixedSize => false;

    bool IList.IsReadOnly => false;

    int ICollection.Count => Count;

    bool ICollection.IsSynchronized => false;

    // Synchronization root for this object.
    object ICollection.SyncRoot
    {
        get
        {
            if (_syncRoot == null)
            {
                Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
            }

            return _syncRoot;
        }
    }

    bool ICollection<T>.IsReadOnly => false;

    /// <inheritdoc />
    ReadOnlySpan<T> IReadOnlyPooledList<T>.Span => Span;

    /// <summary>
    /// Gets or sets the element at the given index.
    /// </summary>
    public T this[int index]
    {
        get
        {
            // Following trick can reduce the range check by one
            if ((uint)index >= (uint)Count)
            {
                OtherThrowHelper.ThrowArgumentOutOfRange_IndexException();
            }

            return _items[index];
        }

        set
        {
            if ((uint)index >= (uint)Count)
            {
                OtherThrowHelper.ThrowArgumentOutOfRange_IndexException();
            }

            _items[index] = value;
            _version++;
        }
    }

    object IList.this[int index]
    {
        get => this[index];
        set
        {
            OtherThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);

            try
            {
                this[index] = (T)value;
            }
            catch (InvalidCastException)
            {
                OtherThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));
            }
        }
    }

    /// <summary>
    /// Returns the internal buffers to the ArrayPool.
    /// </summary>
    public void Dispose()
    {
        ReturnArray();
        Count = 0;
        _version++;
    }

    /// <summary>
    /// Adds the given object to the end of this list. The size of the list is
    /// increased by one. If required, the capacity of the list is doubled
    /// before adding the new element.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        _version++;
        int size = Count;
        if ((uint)size < (uint)_items.Length)
        {
            Count = size + 1;
            _items[size] = item;
        }
        else
        {
            AddWithResize(item);
        }
    }

    /// <summary>
    /// Clears the contents of the PooledList.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        _version++;
        int size = Count;
        Count = 0;

        if (size > 0 && _clearOnFree)

            // Clear the elements so that the gc can reclaim the references.
        {
            Array.Clear(_items, 0, size);
        }
    }

    /// <summary>
    /// Contains returns true if the specified element is in the List.
    /// It does a linear, O(n) search.  Equality is determined by calling
    /// EqualityComparer{T}.Default.Equals.
    /// </summary>
    public bool Contains(T item)
    {
        // PERF: IndexOf calls Array.IndexOf, which internally
        // calls EqualityComparer<T>.Default.IndexOf, which
        // is specialized for different types. This
        // boosts performance since instead of making a
        // virtual method call each iteration of the loop,
        // via EqualityComparer<T>.Default.Equals, we
        // only make one virtual call to EqualityComparer.IndexOf.

        return Count != 0 && IndexOf(item) != -1;
    }

    /// <summary>
    /// Returns the index of the first occurrence of a given value in
    /// this list. The list is searched forwards from beginning to end.
    /// </summary>
    public int IndexOf(T item)
    {
        return Array.IndexOf(_items, item, 0, Count);
    }

    /// <summary>
    /// Inserts an element into this list at a given index. The size of the list
    /// is increased by one. If required, the capacity of the list is doubled
    /// before inserting the new element.
    /// </summary>
    public void Insert(int index, T item)
    {
        // Note that insertions at the end are legal.
        if ((uint)index > (uint)Count)
        {
            OtherThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index,
            ExceptionResource.ArgumentOutOfRange_ListInsert);
        }

        if (Count == _items.Length)
        {
            EnsureCapacity(Count + 1);
        }

        if (index < Count)
        {
            Array.Copy(_items, index, _items, index + 1, Count - index);
        }

        _items[index] = item;
        Count++;
        _version++;
    }

    // Removes the element at the given index. The size of the list is
    // decreased by one.
    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if (index >= 0)
        {
            RemoveAt(index);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes the element at the given index. The size of the list is
    /// decreased by one.
    /// </summary>
    public void RemoveAt(int index)
    {
        if ((uint)index >= (uint)Count)
        {
            OtherThrowHelper.ThrowArgumentOutOfRange_IndexException();
        }

        Count--;
        if (index < Count)
        {
            Array.Copy(_items, index + 1, _items, index, Count - index);
        }

        _version++;

        if (_clearOnFree)

            // Clear the removed element so that the gc can reclaim the reference.
        {
            _items[Count] = default;
        }
    }

    /// <summary>
    /// Adds the elements of the given collection to the end of this list. If
    /// required, the capacity of the list is increased to twice the previous
    /// capacity or the new size, whichever is larger.
    /// </summary>
    public void AddRange(IEnumerable<T> collection)
    {
        InsertRange(Count, collection);
    }

    /// <summary>
    /// Adds the elements of the given array to the end of this list. If
    /// required, the capacity of the list is increased to twice the previous
    /// capacity or the new size, whichever is larger.
    /// </summary>
    public void AddRange(T[] array)
    {
        AddRange(array.AsSpan());
    }

    /// <summary>
    /// Adds the elements of the given <see cref="ReadOnlySpan{T}" /> to the end of this list. If
    /// required, the capacity of the list is increased to twice the previous
    /// capacity or the new size, whichever is larger.
    /// </summary>
    public void AddRange(ReadOnlySpan<T> span)
    {
        Span<T> newSpan = InsertSpan(Count, span.Length, false);
        span.CopyTo(newSpan);
    }

    /// <summary>
    /// Advances the <see cref="Count" /> by the number of items specified,
    /// increasing the capacity if required, then returns a Span representing
    /// the set of items to be added, allowing direct writes to that section
    /// of the collection.
    /// </summary>
    /// <param name="count">The number of items to add.</param>
    public Span<T> AddSpan(int count)
    {
        return InsertSpan(Count, count);
    }

    public ReadOnlyCollection<T> AsReadOnly()
    {
        return new ReadOnlyCollection<T>(this);
    }

    /// <summary>
    /// Searches a section of the list for a given element using a binary search
    /// algorithm.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Elements of the list are compared to the search value using
    ///     the given IComparer interface. If comparer is null, elements of
    ///     the list are compared to the search value using the IComparable
    ///     interface, which in that case must be implemented by all elements of the
    ///     list and the given search value. This method assumes that the given
    ///     section of the list is already sorted; if this is not the case, the
    ///     result will be incorrect.
    ///     </para>
    ///     <para>
    ///     The method returns the index of the given value in the list. If the
    ///     list does not contain the given value, the method returns a negative
    ///     integer. The bitwise complement operator (~) can be applied to a
    ///     negative result to produce the index of the first element (if any) that
    ///     is larger than the given search value. This is also the index at which
    ///     the search value should be inserted into the list in order for the list
    ///     to remain sorted.
    ///     </para>
    /// </remarks>
    public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
    {
        if (index < 0)
        {
            OtherThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
        }

        if (count < 0)
        {
            OtherThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count,
            ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        if (Count - index < count)
        {
            OtherThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
        }

        return Array.BinarySearch(_items, index, count, item, comparer);
    }

    /// <summary>
    /// Searches the list for a given element using a binary search
    /// algorithm. If the item implements <see cref="IComparable{T}" />
    /// then that is used for comparison, otherwise <see cref="Comparer{T}.Default" /> is used.
    /// </summary>
    public int BinarySearch(T item)
    {
        return BinarySearch(0, Count, item, null);
    }

    /// <summary>
    /// Searches the list for a given element using a binary search
    /// algorithm. If the item implements <see cref="IComparable{T}" />
    /// then that is used for comparison, otherwise <see cref="Comparer{T}.Default" /> is used.
    /// </summary>
    public int BinarySearch(T item, IComparer<T> comparer)
    {
        return BinarySearch(0, Count, item, comparer);
    }

    public PooledList<TOutput> ConvertAll<TOutput>(Func<T, TOutput> converter)
    {
        if (converter == null)
        {
            OtherThrowHelper.ThrowArgumentNullException(ExceptionArgument.converter);
        }

        PooledList<TOutput> list = new PooledList<TOutput>(Count);
        for (int i = 0; i < Count; i++)
        {
            list._items[i] = converter(_items[i]);
        }

        list.Count = Count;
        return list;
    }

    /// <summary>
    /// Copies this list to the given span.
    /// </summary>
    public void CopyTo(Span<T> span)
    {
        if (span.Length < Count)
        {
            throw new ArgumentException("Destination span is shorter than the list to be copied.");
        }

        Span.CopyTo(span);
    }

    public bool Exists(Func<T, bool> match)
    {
        return FindIndex(match) != -1;
    }

    public bool TryFind(Func<T, bool> match, out T result)
    {
        if (match == null)
        {
            OtherThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
        }

        for (int i = 0; i < Count; i++)
        {
            if (match(_items[i]))
            {
                result = _items[i];
                return true;
            }
        }

        result = default;
        return false;
    }

    public PooledList<T> FindAll(Func<T, bool> match)
    {
        if (match == null)
        {
            OtherThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
        }

        PooledList<T> list = new PooledList<T>();
        for (int i = 0; i < Count; i++)
        {
            if (match(_items[i]))
            {
                list.Add(_items[i]);
            }
        }

        return list;
    }

    public int FindIndex(Func<T, bool> match)
    {
        return FindIndex(0, Count, match);
    }

    public int FindIndex(int startIndex, Func<T, bool> match)
    {
        return FindIndex(startIndex, Count - startIndex, match);
    }

    public int FindIndex(int startIndex, int count, Func<T, bool> match)
    {
        if ((uint)startIndex > (uint)Count)
        {
            OtherThrowHelper.ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_Index();
        }

        if (count < 0 || startIndex > Count - count)
        {
            OtherThrowHelper.ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count();
        }

        if (match is null)
        {
            OtherThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
        }

        int endIndex = startIndex + count;
        for (int i = startIndex; i < endIndex; i++)
        {
            if (match(_items[i]))
            {
                return i;
            }
        }

        return -1;
    }

    public bool TryFindLast(Func<T, bool> match, out T result)
    {
        if (match is null)
        {
            OtherThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
        }

        for (int i = Count - 1; i >= 0; i--)
        {
            if (match(_items[i]))
            {
                result = _items[i];
                return true;
            }
        }

        result = default;
        return false;
    }

    public int FindLastIndex(Func<T, bool> match)
    {
        return FindLastIndex(Count - 1, Count, match);
    }

    public int FindLastIndex(int startIndex, Func<T, bool> match)
    {
        return FindLastIndex(startIndex, startIndex + 1, match);
    }

    public int FindLastIndex(int startIndex, int count, Func<T, bool> match)
    {
        if (match == null)
        {
            OtherThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
        }

        if (Count == 0)
        {
            // Special case for 0 length List
            if (startIndex != -1)
            {
                OtherThrowHelper.ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_Index();
            }
        }
        else
        {
            // Make sure we're not out of range
            if ((uint)startIndex >= (uint)Count)
            {
                OtherThrowHelper.ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_Index();
            }
        }

        // 2nd half of this also catches when startIndex == MAXINT, so MAXINT - 0 + 1 == -1, which is < 0.
        if (count < 0 || startIndex - count + 1 < 0)
        {
            OtherThrowHelper.ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count();
        }

        int endIndex = startIndex - count;
        for (int i = startIndex; i > endIndex; i--)
        {
            if (match(_items[i]))
            {
                return i;
            }
        }

        return -1;
    }

    public void ForEach(Action<T> action)
    {
        if (action == null)
        {
            OtherThrowHelper.ThrowArgumentNullException(ExceptionArgument.action);
        }

        int version = _version;
        for (int i = 0; i < Count; i++)
        {
            if (version != _version)
            {
                break;
            }

            action(_items[i]);
        }

        if (version != _version)
        {
            OtherThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
        }
    }

    /// <summary>
    /// Returns an enumerator for this list with the given
    /// permission for removal of elements. If modifications made to the list
    /// while an enumeration is in progress, the MoveNext and
    /// GetObject methods of the enumerator will throw an exception.
    /// </summary>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    /// <summary>
    /// Equivalent to PooledList.Span.Slice(index, count).
    /// </summary>
    public Span<T> GetRange(int index, int count)
    {
        if (index < 0)
        {
            OtherThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
        }

        if (count < 0)
        {
            OtherThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count,
            ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        if (Count - index < count)
        {
            OtherThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
        }

        return Span.Slice(index, count);
    }

    /// <summary>
    /// Returns the index of the first occurrence of a given value in a range of
    /// this list. The list is searched forwards, starting at index
    /// index and ending at count number of elements.
    /// </summary>
    public int IndexOf(T item, int index)
    {
        if (index > Count)
        {
            OtherThrowHelper.ThrowArgumentOutOfRange_IndexException();
        }

        return Array.IndexOf(_items, item, index, Count - index);
    }

    /// <summary>
    /// Returns the index of the first occurrence of a given value in a range of
    /// this list. The list is searched forwards, starting at index
    /// index and upto count number of elements.
    /// </summary>
    public int IndexOf(T item, int index, int count)
    {
        if (index > Count)
        {
            OtherThrowHelper.ThrowArgumentOutOfRange_IndexException();
        }

        if (count < 0 || index > Count - count)
        {
            OtherThrowHelper.ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count();
        }

        return Array.IndexOf(_items, item, index, count);
    }

    /// <summary>
    /// Inserts the elements of the given collection at a given index. If
    /// required, the capacity of the list is increased to twice the previous
    /// capacity or the new size, whichever is larger.  Ranges may be added
    /// to the end of the list by setting index to the List's size.
    /// </summary>
    public void InsertRange(int index, IEnumerable<T> collection)
    {
        if ((uint)index > (uint)Count)
        {
            OtherThrowHelper.ThrowArgumentOutOfRange_IndexException();
        }

        switch (collection)
        {
            case null:
                OtherThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
                break;

            case ICollection<T> c:
                int count = c.Count;
                if (count > 0)
                {
                    EnsureCapacity(Count + count);
                    if (index < Count)
                    {
                        Array.Copy(_items, index, _items, index + count, Count - index);
                    }

                    // If we're inserting a List into itself, we want to be able to deal with that.
                    if (this == c)
                    {
                        // Copy first part of _items to insert location
                        Array.Copy(_items, 0, _items, index, index);

                        // Copy last part of _items back to inserted location
                        Array.Copy(_items, index + count, _items, index * 2, Count - index);
                    }
                    else
                    {
                        c.CopyTo(_items, index);
                    }

                    Count += count;
                }

                break;

            default:
                using (IEnumerator<T> en = collection.GetEnumerator())
                {
                    while (en.MoveNext())
                    {
                        Insert(index++, en.Current);
                    }
                }

                break;
        }

        _version++;
    }

    /// <summary>
    /// Inserts the elements of the given collection at a given index. If
    /// required, the capacity of the list is increased to twice the previous
    /// capacity or the new size, whichever is larger.  Ranges may be added
    /// to the end of the list by setting index to the List's size.
    /// </summary>
    public void InsertRange(int index, ReadOnlySpan<T> span)
    {
        Span<T> newSpan = InsertSpan(index, span.Length, false);
        span.CopyTo(newSpan);
    }

    /// <summary>
    /// Inserts the elements of the given collection at a given index. If
    /// required, the capacity of the list is increased to twice the previous
    /// capacity or the new size, whichever is larger.  Ranges may be added
    /// to the end of the list by setting index to the List's size.
    /// </summary>
    public void InsertRange(int index, T[] array)
    {
        if (array is null)
        {
            OtherThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
        }

        InsertRange(index, array.AsSpan());
    }

    /// <summary>
    /// Advances the <see cref="Count" /> by the number of items specified,
    /// increasing the capacity if required, then returns a Span representing
    /// the set of items to be added, allowing direct writes to that section
    /// of the collection.
    /// </summary>
    public Span<T> InsertSpan(int index, int count)
    {
        return InsertSpan(index, count, true);
    }

    /// <summary>
    /// Returns the index of the last occurrence of a given value in a range of
    /// this list. The list is searched backwards, starting at the end
    /// and ending at the first element in the list.
    /// </summary>
    public int LastIndexOf(T item)
    {
        if (Count == 0)

            // Special case for empty list
        {
            return -1;
        }

        return LastIndexOf(item, Count - 1, Count);
    }

    /// <summary>
    /// Returns the index of the last occurrence of a given value in a range of
    /// this list. The list is searched backwards, starting at index
    /// index and ending at the first element in the list.
    /// </summary>
    public int LastIndexOf(T item, int index)
    {
        if (index >= Count)
        {
            OtherThrowHelper.ThrowArgumentOutOfRange_IndexException();
        }

        return LastIndexOf(item, index, index + 1);
    }

    /// <summary>
    /// Returns the index of the last occurrence of a given value in a range of
    /// this list. The list is searched backwards, starting at index
    /// index and upto count elements
    /// </summary>
    public int LastIndexOf(T item, int index, int count)
    {
        if (Count != 0 && index < 0)
        {
            OtherThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
        }

        if (Count != 0 && count < 0)
        {
            OtherThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count,
            ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        if (Count == 0)

            // Special case for empty list
        {
            return -1;
        }

        if (index >= Count)
        {
            OtherThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index,
            ExceptionResource.ArgumentOutOfRange_BiggerThanCollection);
        }

        if (count > index + 1)
        {
            OtherThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count,
            ExceptionResource.ArgumentOutOfRange_BiggerThanCollection);
        }

        return Array.LastIndexOf(_items, item, index, count);
    }

    /// <summary>
    /// This method removes all items which match the predicate.
    /// The complexity is O(n).
    /// </summary>
    public int RemoveAll(Func<T, bool> match)
    {
        if (match == null)
        {
            OtherThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
        }

        int freeIndex = 0; // the first free slot in items array

        // Find the first item which needs to be removed.
        while (freeIndex < Count && !match(_items[freeIndex]))
        {
            freeIndex++;
        }

        if (freeIndex >= Count)
        {
            return 0;
        }

        int current = freeIndex + 1;
        while (current < Count)
        {
            // Find the first item which needs to be kept.
            while (current < Count && match(_items[current]))
            {
                current++;
            }

            if (current < Count)

                // copy item to the free slot.
            {
                _items[freeIndex++] = _items[current++];
            }
        }

        if (_clearOnFree)

            // Clear the removed elements so that the gc can reclaim the references.
        {
            Array.Clear(_items, freeIndex, Count - freeIndex);
        }

        int result = Count - freeIndex;
        Count = freeIndex;
        _version++;
        return result;
    }

    /// <summary>
    /// Removes a range of elements from this list.
    /// </summary>
    public void RemoveRange(int index, int count)
    {
        if (index < 0)
        {
            OtherThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
        }

        if (count < 0)
        {
            OtherThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count,
            ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        if (Count - index < count)
        {
            OtherThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
        }

        if (count > 0)
        {
            Count -= count;
            if (index < Count)
            {
                Array.Copy(_items, index + count, _items, index, Count - index);
            }

            _version++;

            if (_clearOnFree)

                // Clear the removed elements so that the gc can reclaim the references.
            {
                Array.Clear(_items, Count, count);
            }
        }
    }

    /// <summary>
    /// Reverses the elements in this list.
    /// </summary>
    public void Reverse()
    {
        Reverse(0, Count);
    }

    /// <summary>
    /// Reverses the elements in a range of this list. Following a call to this
    /// method, an element in the range given by index and count
    /// which was previously located at index i will now be located at
    /// index index + (index + count - i - 1).
    /// </summary>
    public void Reverse(int index, int count)
    {
        if (index < 0)
        {
            OtherThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
        }

        if (count < 0)
        {
            OtherThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count,
            ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        if (Count - index < count)
        {
            OtherThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
        }

        if (count > 1)
        {
            Array.Reverse(_items, index, count);
        }

        _version++;
    }

    /// <summary>
    /// Sorts the elements in this list.  Uses the default comparer and
    /// Array.Sort.
    /// </summary>
    public void Sort()
    {
        Sort(0, Count, null);
    }

    /// <summary>
    /// Sorts the elements in this list.  Uses Array.Sort with the
    /// provided comparer.
    /// </summary>
    /// <param name="comparer"></param>
    public void Sort(IComparer<T> comparer)
    {
        Sort(0, Count, comparer);
    }

    /// <summary>
    /// Sorts the elements in a section of this list. The sort compares the
    /// elements to each other using the given IComparer interface. If
    /// comparer is null, the elements are compared to each other using
    /// the IComparable interface, which in that case must be implemented by all
    /// elements of the list.
    /// This method uses the Array.Sort method to sort the elements.
    /// </summary>
    public void Sort(int index, int count, IComparer<T> comparer)
    {
        if (index < 0)
        {
            OtherThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
        }

        if (count < 0)
        {
            OtherThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count,
            ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        if (Count - index < count)
        {
            OtherThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
        }

        if (count > 1)
        {
            Array.Sort(_items, index, count, comparer);
        }

        _version++;
    }

    public void Sort(Func<T, T, int> comparison)
    {
        if (comparison == null)
        {
            OtherThrowHelper.ThrowArgumentNullException(ExceptionArgument.comparison);
        }

        if (Count > 1)

            // List<T> uses ArraySortHelper here but since it's an internal class,
            // we're creating an IComparer<T> using the comparison function to avoid
            // duplicating all that code.
        {
            Array.Sort(_items, 0, Count, new Comparer(comparison));
        }

        _version++;
    }

    /// <summary>
    /// ToArray returns an array containing the contents of the List.
    /// This requires copying the List, which is an O(n) operation.
    /// </summary>
    public T[] ToArray()
    {
        if (Count == 0)
        {
            return _emptyArray;
        }

        return Span.ToArray();
    }

    /// <summary>
    /// Sets the capacity of this list to the size of the list. This method can
    /// be used to minimize a list's memory overhead once it is known that no
    /// new elements will be added to the list. To completely clear a list and
    /// release all memory referenced by the list, execute the following
    /// statements:
    /// <code>
    /// list.Clear();
    /// list.TrimExcess();
    /// </code>
    /// </summary>
    public void TrimExcess()
    {
        int threshold = (int)(_items.Length * 0.9);
        if (Count < threshold)
        {
            Capacity = Count;
        }
    }

    public bool TrueForAll(Func<T, bool> match)
    {
        if (match == null)
        {
            OtherThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
        }

        for (int i = 0; i < Count; i++)
        {
            if (!match(_items[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsCompatibleObject(object value)
    {
        // Non-null values are fine.  Only accept nulls if T is a class or Nullable<U>.
        // Note that default(T) is not equal to null for value types except when T is Nullable<U>. 
        return value is T || value == null && default(T) == null;
    }

    private static bool ShouldClear(ClearMode mode)
    {
#if NETCOREAPP2_1
            return mode == ClearMode.Always
                || (mode == ClearMode.Auto && RuntimeHelpers.IsReferenceOrContainsReferences<T>());
#else
        return mode != ClearMode.Never;
#endif
    }

    void IDeserializationCallback.OnDeserialization(object sender)
    {
        // We can't serialize array pools, so deserialized PooledLists will
        // have to use the shared pool, even if they were using a custom pool
        // before serialization.
        _pool = ArrayPool<T>.Shared;
    }

    int IList.Add(object value)
    {
        OtherThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.item);

        try
        {
            Add((T)value);
        }
        catch (InvalidCastException)
        {
            OtherThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));
        }

        return Count - 1;
    }

    bool IList.Contains(object value)
    {
        if (IsCompatibleObject(value))
        {
            return Contains((T)value);
        }

        return false;
    }

    // Copies this List into array, which must be of a 
    // compatible array type.  
    void ICollection.CopyTo(Array array, int index)
    {
        if (array != null && array.Rank != 1)
        {
            OtherThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
        }

        try
        {
            // Array.Copy will check for NULL.
            Array.Copy(_items, 0, array, index, Count);
        }
        catch (ArrayTypeMismatchException)
        {
            OtherThrowHelper.ThrowArgumentException_Argument_InvalidArrayType();
        }
    }

    int IList.IndexOf(object value)
    {
        if (IsCompatibleObject(value))
        {
            return IndexOf((T)value);
        }

        return -1;
    }

    void IList.Insert(int index, object value)
    {
        OtherThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.item);

        try
        {
            Insert(index, (T)value);
        }
        catch (InvalidCastException)
        {
            OtherThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));
        }
    }

    void IList.Remove(object value)
    {
        if (IsCompatibleObject(value))
        {
            Remove((T)value);
        }
    }

    void ICollection<T>.CopyTo(T[] array, int arrayIndex)
    {
        Array.Copy(_items, 0, array, arrayIndex, Count);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new Enumerator(this);
    }

    // Non-inline from List.Add to improve its code quality as uncommon path
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddWithResize(T item)
    {
        int size = Count;
        EnsureCapacity(size + 1);
        Count = size + 1;
        _items[size] = item;
    }

    /// <summary>
    /// Ensures that the capacity of this list is at least the given minimum
    /// value. If the current capacity of the list is less than min, the
    /// capacity is increased to twice the current capacity or to min,
    /// whichever is larger.
    /// </summary>
    private void EnsureCapacity(int min)
    {
        if (_items.Length < min)
        {
            int newCapacity = _items.Length == 0 ? DEFAULT_CAPACITY : _items.Length * 2;

            // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
            // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
            if ((uint)newCapacity > MAX_ARRAY_LENGTH)
            {
                newCapacity = MAX_ARRAY_LENGTH;
            }

            if (newCapacity < min)
            {
                newCapacity = min;
            }

            Capacity = newCapacity;
        }
    }

    private Span<T> InsertSpan(int index, int count, bool clearOutput)
    {
        EnsureCapacity(Count + count);

        if (index < Count)
        {
            Array.Copy(_items, index, _items, index + count, Count - index);
        }

        Count += count;
        _version++;

        Span<T> output = _items.AsSpan(index, count);

        if (clearOutput && _clearOnFree)
        {
            output.Clear();
        }

        return output;
    }

    private void ReturnArray()
    {
        if (_items.Length == 0)
        {
            return;
        }

        try
        {
            // Clear the elements so that the gc can reclaim the references.
            _pool.Return(_items, _clearOnFree);
        }
        catch (ArgumentException)
        {
            // oh well, the array pool didn't like our array
        }

        _items = _emptyArray;
    }

    public struct Enumerator : IEnumerator<T>
    {
        private readonly PooledList<T> _list;
        private int _index;
        private readonly int _version;

        internal Enumerator(PooledList<T> list)
        {
            _list = list;
            _index = 0;
            _version = list._version;
            Current = default;
        }

        public void Dispose()
        {
            // nothing to dispose
        }

        public bool MoveNext()
        {
            PooledList<T>? localList = _list;

            if (_version == localList._version && (uint)_index < (uint)localList.Count)
            {
                Current = localList._items[_index];
                _index++;
                return true;
            }

            return MoveNextRare();
        }

        private bool MoveNextRare()
        {
            if (_version != _list._version)
            {
                OtherThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
            }

            _index = _list.Count + 1;
            Current = default;
            return false;
        }

        public T Current { get; private set; }

        object IEnumerator.Current
        {
            get
            {
                if (_index == 0 || _index == _list.Count + 1)
                {
                    OtherThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
                }

                return Current;
            }
        }

        void IEnumerator.Reset()
        {
            if (_version != _list._version)
            {
                OtherThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
            }

            _index = 0;
            Current = default;
        }
    }

    private readonly struct Comparer : IComparer<T>
    {
        private readonly Func<T, T, int> _comparison;

        public Comparer(Func<T, T, int> comparison)
        {
            _comparison = comparison;
        }

        public int Compare(T x, T y)
        {
            return _comparison(x, y);
        }
    }

    #region Constructors

    /// <summary>
    /// Constructs a PooledList. The list is initially empty and has a capacity
    /// of zero. Upon adding the first element to the list the capacity is
    /// increased to DefaultCapacity, and then increased in multiples of two
    /// as required.
    /// </summary>
    public PooledList() : this(ClearMode.Auto, ArrayPool<T>.Shared)
    {
    }

    /// <summary>
    /// Constructs a PooledList. The list is initially empty and has a capacity
    /// of zero. Upon adding the first element to the list the capacity is
    /// increased to DefaultCapacity, and then increased in multiples of two
    /// as required.
    /// </summary>
    public PooledList(ClearMode clearMode) : this(clearMode, ArrayPool<T>.Shared)
    {
    }

    /// <summary>
    /// Constructs a PooledList. The list is initially empty and has a capacity
    /// of zero. Upon adding the first element to the list the capacity is
    /// increased to DefaultCapacity, and then increased in multiples of two
    /// as required.
    /// </summary>
    public PooledList(ArrayPool<T> customPool) : this(ClearMode.Auto, customPool)
    {
    }

    /// <summary>
    /// Constructs a PooledList. The list is initially empty and has a capacity
    /// of zero. Upon adding the first element to the list the capacity is
    /// increased to DefaultCapacity, and then increased in multiples of two
    /// as required.
    /// </summary>
    public PooledList(ClearMode clearMode, ArrayPool<T> customPool)
    {
        _items = _emptyArray;
        _pool = customPool ?? ArrayPool<T>.Shared;
        _clearOnFree = ShouldClear(clearMode);
    }

    /// <summary>
    /// Constructs a List with a given initial capacity. The list is
    /// initially empty, but will have room for the given number of elements
    /// before any reallocations are required.
    /// </summary>
    public PooledList(int capacity) : this(capacity, ClearMode.Auto, ArrayPool<T>.Shared)
    {
    }

    /// <summary>
    /// Constructs a List with a given initial capacity. The list is
    /// initially empty, but will have room for the given number of elements
    /// before any reallocations are required.
    /// </summary>
    public PooledList(int capacity, bool sizeToCapacity) : this(capacity,
    ClearMode.Auto,
    ArrayPool<T>.Shared,
    sizeToCapacity)
    {
    }

    /// <summary>
    /// Constructs a List with a given initial capacity. The list is
    /// initially empty, but will have room for the given number of elements
    /// before any reallocations are required.
    /// </summary>
    public PooledList(int capacity, ClearMode clearMode) : this(capacity, clearMode, ArrayPool<T>.Shared)
    {
    }

    /// <summary>
    /// Constructs a List with a given initial capacity. The list is
    /// initially empty, but will have room for the given number of elements
    /// before any reallocations are required.
    /// </summary>
    public PooledList(int capacity, ClearMode clearMode, bool sizeToCapacity) : this(capacity,
    clearMode,
    ArrayPool<T>.Shared,
    sizeToCapacity)
    {
    }

    /// <summary>
    /// Constructs a List with a given initial capacity. The list is
    /// initially empty, but will have room for the given number of elements
    /// before any reallocations are required.
    /// </summary>
    public PooledList(int capacity, ArrayPool<T> customPool) : this(capacity, ClearMode.Auto, customPool)
    {
    }

    /// <summary>
    /// Constructs a List with a given initial capacity. The list is
    /// initially empty, but will have room for the given number of elements
    /// before any reallocations are required.
    /// </summary>
    public PooledList(int capacity, ArrayPool<T> customPool, bool sizeToCapacity) : this(capacity,
    ClearMode.Auto,
    customPool,
    sizeToCapacity)
    {
    }

    /// <summary>
    /// Constructs a List with a given initial capacity. The list is
    /// initially empty, but will have room for the given number of elements
    /// before any reallocations are required.
    /// </summary>
    /// <param name="sizeToCapacity">
    /// If true, Count of list equals capacity. Depending on ClearMode, rented items may or may
    /// not hold dirty values.
    /// </param>
    public PooledList(int capacity, ClearMode clearMode, ArrayPool<T> customPool, bool sizeToCapacity = false)
    {
        if (capacity < 0)
        {
            OtherThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity,
            ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        _pool = customPool ?? ArrayPool<T>.Shared;
        _clearOnFree = ShouldClear(clearMode);

        if (capacity == 0)
        {
            _items = _emptyArray;
        }
        else
        {
            _items = _pool.Rent(capacity);
        }

        if (sizeToCapacity)
        {
            Count = capacity;
            if (clearMode != ClearMode.Never)
            {
                Array.Clear(_items, 0, Count);
            }
        }
    }

    /// <summary>
    /// Constructs a PooledList, copying the contents of the given collection. The
    /// size and capacity of the new list will both be equal to the size of the
    /// given collection.
    /// </summary>
    public PooledList(T[] array) : this(array.AsSpan(), ClearMode.Auto, ArrayPool<T>.Shared)
    {
    }

    /// <summary>
    /// Constructs a PooledList, copying the contents of the given collection. The
    /// size and capacity of the new list will both be equal to the size of the
    /// given collection.
    /// </summary>
    public PooledList(T[] array, ClearMode clearMode) : this(array.AsSpan(), clearMode, ArrayPool<T>.Shared)
    {
    }

    /// <summary>
    /// Constructs a PooledList, copying the contents of the given collection. The
    /// size and capacity of the new list will both be equal to the size of the
    /// given collection.
    /// </summary>
    public PooledList(T[] array, ArrayPool<T> customPool) : this(array.AsSpan(), ClearMode.Auto, customPool)
    {
    }

    /// <summary>
    /// Constructs a PooledList, copying the contents of the given collection. The
    /// size and capacity of the new list will both be equal to the size of the
    /// given collection.
    /// </summary>
    public PooledList(T[] array, ClearMode clearMode, ArrayPool<T> customPool) : this(array.AsSpan(),
    clearMode,
    customPool)
    {
    }

    /// <summary>
    /// Constructs a PooledList, copying the contents of the given collection. The
    /// size and capacity of the new list will both be equal to the size of the
    /// given collection.
    /// </summary>
    public PooledList(ReadOnlySpan<T> span) : this(span, ClearMode.Auto, ArrayPool<T>.Shared)
    {
    }

    /// <summary>
    /// Constructs a PooledList, copying the contents of the given collection. The
    /// size and capacity of the new list will both be equal to the size of the
    /// given collection.
    /// </summary>
    public PooledList(ReadOnlySpan<T> span, ClearMode clearMode) : this(span, clearMode, ArrayPool<T>.Shared)
    {
    }

    /// <summary>
    /// Constructs a PooledList, copying the contents of the given collection. The
    /// size and capacity of the new list will both be equal to the size of the
    /// given collection.
    /// </summary>
    public PooledList(ReadOnlySpan<T> span, ArrayPool<T> customPool) : this(span, ClearMode.Auto, customPool)
    {
    }

    /// <summary>
    /// Constructs a PooledList, copying the contents of the given collection. The
    /// size and capacity of the new list will both be equal to the size of the
    /// given collection.
    /// </summary>
    public PooledList(ReadOnlySpan<T> span, ClearMode clearMode, ArrayPool<T> customPool)
    {
        _pool = customPool ?? ArrayPool<T>.Shared;
        _clearOnFree = ShouldClear(clearMode);

        int count = span.Length;
        if (count == 0)
        {
            _items = _emptyArray;
        }
        else
        {
            _items = _pool.Rent(count);
            span.CopyTo(_items);
            Count = count;
        }
    }

    /// <summary>
    /// Constructs a PooledList, copying the contents of the given collection. The
    /// size and capacity of the new list will both be equal to the size of the
    /// given collection.
    /// </summary>
    public PooledList(IEnumerable<T> collection) : this(collection, ClearMode.Auto, ArrayPool<T>.Shared)
    {
    }

    /// <summary>
    /// Constructs a PooledList, copying the contents of the given collection. The
    /// size of the new list will be equal to the size of the given collection
    /// and the capacity will be equal to suggestCapacity
    /// </summary>
    public PooledList(IEnumerable<T> collection, int suggestCapacity) : this(collection,
    ClearMode.Auto,
    ArrayPool<T>.Shared,
    suggestCapacity)
    {
    }

    /// <summary>
    /// Constructs a PooledList, copying the contents of the given collection. The
    /// size and capacity of the new list will both be equal to the size of the
    /// given collection.
    /// </summary>
    public PooledList(IEnumerable<T> collection, ClearMode clearMode) : this(collection,
    clearMode,
    ArrayPool<T>.Shared)
    {
    }

    /// <summary>
    /// Constructs a PooledList, copying the contents of the given collection. The
    /// size and capacity of the new list will both be equal to the size of the
    /// given collection.
    /// </summary>
    public PooledList(IEnumerable<T> collection, ArrayPool<T> customPool) : this(collection,
    ClearMode.Auto,
    customPool)
    {
    }

    /// <summary>
    /// Constructs a PooledList, copying the contents of the given collection. The
    /// size and capacity of the new list will both be equal to the size of the
    /// given collection.
    /// </summary>
    public PooledList(
    IEnumerable<T> collection,
    ClearMode clearMode,
    ArrayPool<T> customPool,
    int suggestCapacity = 0)
    {
        _pool = customPool ?? ArrayPool<T>.Shared;
        _clearOnFree = ShouldClear(clearMode);

        switch (collection)
        {
            case null:
                OtherThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
                break;

            case ICollection<T> c:
            {
                int count = c.Count;
                if (count == 0)
                {
                    _items = _emptyArray;
                }
                else
                {
                    _items = _pool.Rent(count);
                    c.CopyTo(_items, 0);
                    Count = count;
                }

                break;
            }

            case ICollection c:
            {
                int count = c.Count;
                if (count == 0)
                {
                    _items = _emptyArray;
                }
                else
                {
                    _items = _pool.Rent(count);
                    c.CopyTo(_items, 0);
                    Count = count;
                }

                break;
            }

            case IReadOnlyCollection<T> c:
            {
                int count = c.Count;
                if (count == 0)
                {
                    _items = _emptyArray;
                }
                else
                {
                    _items = _pool.Rent(count);
                    Count = 0;
                    using (IEnumerator<T> en = c.GetEnumerator())
                    {
                        while (en.MoveNext())
                        {
                            Add(en.Current);
                        }
                    }
                }

                break;
            }

            default:
                if (suggestCapacity < 0)
                {
                    OtherThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity,
                    ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
                }

                if (suggestCapacity == 0)
                {
                    _items = _emptyArray;
                }
                else
                {
                    _items = _pool.Rent(suggestCapacity);
                }

                using (IEnumerator<T> en = collection.GetEnumerator())
                {
                    while (en.MoveNext())
                    {
                        Add(en.Current);
                    }
                }

                break;
        }
    }

    #endregion
}