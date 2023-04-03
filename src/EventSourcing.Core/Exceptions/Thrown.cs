using System;
using System.Diagnostics.CodeAnalysis;

namespace EventSourcing.Core.Exceptions;

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
}