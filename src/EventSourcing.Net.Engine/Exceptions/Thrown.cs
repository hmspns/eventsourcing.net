using System;
using System.Diagnostics.CodeAnalysis;

namespace EventSourcing.Net.Engine.Exceptions;

internal static class Thrown
{
    [DoesNotReturn]
    internal static void InvalidOperationException(string message, Exception? ex = null)
    {
        throw new InvalidOperationException(message, ex);
    }

    [DoesNotReturn]
    internal static void ArgumentNullException(string paramName, string? message = null)
    {
        throw new ArgumentNullException(paramName, message);
    }
    
    [DoesNotReturn]
    internal static void ArgumentException(string message, string? paramName = null, Exception? innerException = null)
    {
        throw new ArgumentException(message, paramName, innerException);
    }

    [DoesNotReturn]
    internal static void ArgumentOutOfRangeException(string paramName, string message, object actualValue = null)
    {
        throw new ArgumentOutOfRangeException(paramName, actualValue, message);
    }

    [DoesNotReturn]
    internal static void ObjectDisposedException(string objectName)
    {
        throw new ObjectDisposedException(objectName);
    }

    [DoesNotReturn]
    internal static void NotSupportedException(string message)
    {
        throw new NotSupportedException(message);
    }
    
    [DoesNotReturn]
    internal static void IndexOutOfRangeException(string? message = null)
    {
        if (message == null)
        {
            throw new IndexOutOfRangeException();
        }
        throw new IndexOutOfRangeException(message);
    }
}