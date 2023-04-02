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
}