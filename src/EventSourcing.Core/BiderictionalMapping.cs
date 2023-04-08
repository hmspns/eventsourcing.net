using System;
using System.Collections.Generic;
using System.Threading;
using EventSourcing.Core.Exceptions;

namespace EventSourcing.Core;

internal sealed class BidirectionalMapping<TFirst, TSecond> : IDisposable
    where TFirst : notnull
    where TSecond : notnull
{
    private readonly Dictionary<TFirst, TSecond> _first = new();
    private readonly Dictionary<TSecond, TFirst> _second = new();

    private readonly ReaderWriterLockSlim _locker = new(LockRecursionPolicy.NoRecursion);
    
    internal bool TryAdd(TFirst first, TSecond second)
    {
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

    internal bool TryGetValue(TFirst key, out TSecond value)
    {
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
        _locker.Dispose();
    }
}