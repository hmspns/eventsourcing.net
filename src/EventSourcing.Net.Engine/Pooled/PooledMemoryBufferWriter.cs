/*
 * Copyright 2017 itn3000
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using EventSourcing.Net.Engine.Exceptions;

namespace EventSourcing.Net.Engine.Pooled;

/// <summary>
/// Represents an pooled output sink into which <typeparam name="T"/> data can be written.
/// </summary>
public sealed class PooledMemoryBufferWriter<T> : IDisposable, IBufferWriter<T> where T : struct
{
    private readonly ArrayPool<T> _pool;
    private T[] _currentBuffer;
    private int _position;
    private int _length;
    private const int DEFAULT_SIZE = 1024;
    private bool _isDisposed;

    /// <summary>
    /// Initialize new object.
    /// </summary>
    /// <param name="initialCapacity">Initial capacity</param>
    public PooledMemoryBufferWriter(int initialCapacity = DEFAULT_SIZE)
        : this(ArrayPool<T>.Shared, initialCapacity)
    {
    }

    /// <summary>
    /// Initialize new object.
    /// </summary>
    /// <param name="pool">Array pool.</param>
    /// <param name="initialCapacity">initial reserved buffer size.</param>
    public PooledMemoryBufferWriter(ArrayPool<T> pool, int initialCapacity = DEFAULT_SIZE)
    {
        CheckDisposed();
        if (pool == null)
        {
            Thrown.ArgumentNullException(nameof(pool));
        }

        if (initialCapacity < 0)
        {
            Thrown.ArgumentOutOfRangeException(nameof(initialCapacity), "Size must be greater than 0");
        }

        _pool = pool;
        _position = 0;
        _length = 0;
        ResizeBuffer(initialCapacity);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        CheckDisposed();
        if (_position + count > _currentBuffer.Length)
        {
            Thrown.ArgumentOutOfRangeException(nameof(count), "Current position + count will overflow buffer size");
        }

        _position += count;
        if (_length < _position)
        {
            _length = _position;
        }
    }

    /// <summary>return buffer to pool and reset buffer status</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }
        
        if (_currentBuffer != null)
        {
            _pool.Return(_currentBuffer);
            _currentBuffer = null;
            _position = 0;
            _length = 0;
        }

        _isDisposed = true;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Memory<T> GetMemory(int sizeHint = 0)
    {
        CheckDisposed();
        if (sizeHint < 0)
        {
            Thrown.ArgumentOutOfRangeException(nameof(sizeHint), "Size must be greater than 0");
        }

        if (sizeHint == 0)
        {
            sizeHint = DEFAULT_SIZE;
        }

        if (_position + sizeHint > _currentBuffer.Length)
        {
            ResizeBuffer(_position + sizeHint);
        }

        return _currentBuffer.AsMemory(_position, sizeHint);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> GetSpan(int sizeHint = 0)
    {
        CheckDisposed();
        if (sizeHint < 0)
        {
            Thrown.ArgumentOutOfRangeException(nameof(sizeHint), "Size must be greater than 0");
        }

        if (sizeHint == 0)
        {
            sizeHint = DEFAULT_SIZE;
        }

        if (_position + sizeHint > _currentBuffer.Length)
        {
            ResizeBuffer(_position + sizeHint);
        }

        return _currentBuffer.AsSpan(_position, sizeHint);
    }

    /// <summary>Get data as Span.</summary>
    /// <remarks>You must not use returned value outside of stream's lifetime</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<T> ToSpanUnsafe()
    {
        CheckDisposed();
        return _currentBuffer.AsSpan(0, _length);
    }

    /// <summary>Get data as Span.</summary>
    /// <remarks>You must not use returned value outside of stream's lifetime.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyMemory<T> ToMemoryUnsafe()
    {
        CheckDisposed();
        return _currentBuffer.AsMemory(0, _length);
    }

    /// <summary>Reset buffer status.</summary>
    /// <remarks>New buffer with <paramref name="preallocateSize"/> size will be fetched from pool.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset(int preallocateSize)
    {
        CheckDisposed();
        if (preallocateSize < 0)
        {
            Thrown.ArgumentOutOfRangeException(nameof(preallocateSize), "Size must be greater than 0");
        }

        _pool.Return(_currentBuffer);
        _currentBuffer = _pool.Rent(preallocateSize);
        _length = 0;
        _position = 0;
    }

    /// <summary>Reset buffer status.</summary>
    /// <remarks>Buffer will be reused.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset()
    {
        CheckDisposed();
        _length = 0;
        _position = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(_currentBuffer))]
    private void ResizeBuffer(int sizeHint)
    {
        T[] tmp = _pool.Rent(sizeHint);
        if (_currentBuffer != null)
        {
            Buffer.BlockCopy(_currentBuffer, 0, tmp, 0,
            _currentBuffer.Length < tmp.Length ? _currentBuffer.Length : tmp.Length);
            _pool.Return(_currentBuffer);
        }

        _currentBuffer = tmp;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckDisposed()
    {
        if (_isDisposed)
        {
            Thrown.ObjectDisposedException(nameof(PooledMemoryStream));
        }
    }
}