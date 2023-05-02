using System;
using System.Collections.Generic;
using System.Threading;
using EventSourcing.Net.Engine.Exceptions;

namespace EventSourcing.Net.Engine;

internal sealed class BidirectionalMapping<TFirst, TSecond> : IDisposable
    where TFirst : notnull
    where TSecond : notnull
{
    private Dictionary<TFirst, TSecond> _first = new();
    private Dictionary<TSecond, TFirst> _second = new();

    private ReaderWriterLockSlim _locker = new(LockRecursionPolicy.NoRecursion);
    private bool _isDisposed = false;
    
    internal bool TryAdd(TFirst first, TSecond second)
    {
        CheckDisposed();
        if (first == null)
        {
            Thrown.ArgumentNullException(nameof(first));
        }

        if (second == null)
        {
            Thrown.ArgumentNullException(nameof(second));
        }
        
        try
        {
            _locker.EnterWriteLock();
            if (_first.ContainsKey(first) || _second.ContainsKey(second))
            {
                return false;
            }
            
            _first.Add(first, second);
            _second.Add(second, first);
            return true;
        }
        finally
        {
            _locker.ExitWriteLock();
        }
    }

    internal bool TryRemove(TFirst first, TSecond second)
    {
        CheckDisposed();
        if (first == null)
        {
            Thrown.ArgumentNullException(nameof(first));
        }

        if (second == null)
        {
            Thrown.ArgumentNullException(nameof(second));
        }

        try
        {
            _locker.EnterWriteLock();
            bool firstRemoved = _first.Remove(first);
            bool secondRemoved = _second.Remove(second);
            return firstRemoved || secondRemoved;
        }
        finally
        {
            _locker.ExitWriteLock();
        }
    }

    internal bool TryGetValue(TFirst key, out TSecond value)
    {
        CheckDisposed();
        try
        {
            _locker.EnterReadLock();
            return _first.TryGetValue(key, out value);
        }
        finally
        {
            _locker.ExitReadLock();
        }
    }

    internal bool TryGetValue(TSecond key, out TFirst value)
    {
        CheckDisposed();
        try
        {
            _locker.EnterReadLock();
            return _second.TryGetValue(key, out value);
        }
        finally
        {
            _locker.ExitReadLock();
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }
        _locker.Dispose();
        _first = null;
        _second = null;
        _locker = null;
        _isDisposed = true;
    }

    private void CheckDisposed()
    {
        if (_isDisposed)
        {
            Thrown.ObjectDisposedException(nameof(BidirectionalMapping<TFirst,TSecond>));
        }
    }
}