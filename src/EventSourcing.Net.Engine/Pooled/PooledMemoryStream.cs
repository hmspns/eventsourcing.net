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
using System.IO;
using System.Runtime.CompilerServices;
using EventSourcing.Net.Engine.Exceptions;

namespace EventSourcing.Net.Engine.Pooled;

/// <inheritdoc />
public sealed class PooledMemoryStream : Stream
{
    private readonly ArrayPool<byte>? _pool;
    private byte[] _currentBuffer;
    private int _length;
    private int _position;
    private bool _isDisposed;
    
    /// <summary>Initialize new object.</summary>
    public PooledMemoryStream()
        : this(ArrayPool<byte>.Shared)
    {
    }

    /// <summary>Initialize new object with specified ArrayPool.</summary>
    /// <param name="pool">Array pool.</param>
    /// <param name="capacity">Initial capacity.</param>
    public PooledMemoryStream(ArrayPool<byte> pool, int capacity = 256)
    {
        if (pool == null)
        {
            Thrown.ArgumentNullException(nameof(pool));    
        }
        
        _pool = pool;
        _currentBuffer = _pool.Rent(capacity);
        _length = 0;
        CanWrite = true;
    }

    /// <summary>Initialize new object for reading.</summary>
    /// <param name="data">Data that should be read.</param>
    public PooledMemoryStream(byte[] data)
    {
        if (data == null)
        {
            Thrown.ArgumentNullException(nameof(data));
        }
        
        _pool = null;
        _currentBuffer = data;
        _length = data.Length;
        CanWrite = false;
    }

    /// <inheritdoc />
    public override bool CanRead => true;

    /// <inheritdoc />
    public override bool CanSeek => true;

    /// <inheritdoc />
    public override bool CanWrite { get; }

    /// <inheritdoc />
    public override long Length => _length;

    /// <inheritdoc />
    public override long Position
    {
        get => _position;
        set => _position = (int)value;
    }

    /// <inheritdoc />
    public override void Flush()
    {
        CheckDisposed();
    }

    /// <summary>Set stream length.</summary>
    /// <param name="value">New length.</param>
    /// <exception cref="System.NotSupportedException">Stream is readonly.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Value is less than 0 or greater int.MaxValue.</exception>
    public override void SetLength(long value)
    {
        CheckDisposed();
        if (!CanWrite)
        {
            Thrown.NotSupportedException("Stream is readonly");
        }

        if (value > int.MaxValue)
        {
            Thrown.ArgumentOutOfRangeException(nameof(value), "Value should be least than int.MaxValue", value);
        }

        if (value < 0)
        {
            Thrown.ArgumentOutOfRangeException(nameof(value), "Value should be greater than 0", value);
        }

        _length = (int)value;
        if (_currentBuffer == null || _currentBuffer.Length < _length)
        {
            ResizeBuffer(_length);
        }

        if (_position >= _length)
        {
            if (_length == 0)
            {
                _position = 0;
            }
            else
            {
                _position = _length - 1;
            }
        }
    }

    /// <summary>Ensure the buffer size.</summary>
    /// <exception cref="System.NotSupportedException">Stream is readonly.</exception>
    public void Reserve(int capacity)
    {
        CheckDisposed();
        if (!CanWrite)
        {
            Thrown.NotSupportedException("Stream is readonly");
        }

        if (capacity > _currentBuffer.Length)
        {
            ResizeBuffer(capacity);
        }
    }

    /// <summary>Create newly allocated buffer and copy the stream data.</summary>
    public byte[] ToArray()
    {
        CheckDisposed();
        byte[] result = GC.AllocateUninitializedArray<byte>(_length);
        Buffer.BlockCopy(_currentBuffer, 0, result, 0, _length);
        return result;
    }

    /// <summary>Create ArraySegment for current stream data without allocation buffer.</summary>
    /// <remarks>After disposing stream, manipulating returned value (read or write) may cause undefined behavior.</remarks>
    public ArraySegment<byte> ToUnsafeArraySegment()
    {
        CheckDisposed();
        return new ArraySegment<byte>(_currentBuffer, 0, _length);
    }
    
    /// <summary>Get data as Span.</summary>
    /// <remarks>You must not use returned value outside of stream's lifetime</remarks>
    public ReadOnlySpan<byte> ToSpanUnsafe()
    {
        CheckDisposed();
        if (_currentBuffer == null || _length <= 0)
        {
            return Span<byte>.Empty;
        }

        return _currentBuffer.AsSpan(0, _length);
    }

    /// <summary>Get data as Span.</summary>
    /// <remarks>You must not use returned value outside of stream's lifetime.</remarks>
    public ReadOnlyMemory<byte> ToMemoryUnsafe()
    {
        CheckDisposed();
        if (_currentBuffer == null || _length <= 0)
        {
            return Memory<byte>.Empty;
        }

        return _currentBuffer.AsMemory(0, _length);
    }

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin)
    {
        CheckDisposed();
        int oldValue = _position;
        switch ((int)origin)
        {
            case (int)SeekOrigin.Begin:
                _position = (int)offset;
                break;
            
            case (int)SeekOrigin.End:
                _position = _length - (int)offset;
                break;
            
            case (int)SeekOrigin.Current:
                _position += (int)offset;
                break;
            
            default:
                Thrown.InvalidOperationException("Wrong SeekOrigin");
                break;
        }

        if (_position < 0 || _position > _length)
        {
            _position = oldValue;
            Thrown.IndexOutOfRangeException();
        }

        return _position;
    }

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count)
    {
        CheckDisposed();
        int length = count > _length - _position ? _length - _position : count;
        if (length <= 0)
        {
            return 0;
        }
        
        Buffer.BlockCopy(_currentBuffer, _position, buffer, offset, length);
        _position += length;
        return length;
    }

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count)
    {
        CheckDisposed();
        if (!CanWrite)
        {
            Thrown.NotSupportedException("Stream is readonly");
        }

        int endOffset = _position + count;
        if (_currentBuffer == null || endOffset > _currentBuffer.Length)
        {
            ResizeBuffer(endOffset * 2);
        }

        Buffer.BlockCopy(buffer, offset, _currentBuffer, _position, count);
        if (endOffset > _length)
        {
            _length = endOffset;
        }

        _position = endOffset;
    }

    /// <summary>Shrink internal buffer by re-allocating memory.</summary>
    /// <param name="newCapacity">New size.</param>
    /// <exception cref="System.NotSupportedException">Stream is readonly.</exception>
    public void Shrink(int newCapacity)
    {
        CheckDisposed();
        if (!CanWrite)
        {
            Thrown.NotSupportedException("Stream is readonly");
        }

        if (_currentBuffer == null)
        {
            return;
        }

        if (_currentBuffer.Length > newCapacity)
        {
            ResizeBuffer(newCapacity);
        }

        if (newCapacity <= _length)
        {
            _length = newCapacity;
        }
    }
    
    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (_pool != null && _currentBuffer != null)
        {
            _pool.Return(_currentBuffer);
            _currentBuffer = null!;
        }

        _length = 0;
        _position = 0;
        _isDisposed = true;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(_currentBuffer))]
    private void ResizeBuffer(int minimumRequired)
    {
        byte[] tmp = _pool!.Rent(minimumRequired);
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