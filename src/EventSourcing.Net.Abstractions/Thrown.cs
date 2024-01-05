namespace EventSourcing.Net.Abstractions;

using System;
using System.Diagnostics.CodeAnalysis;

internal static class Thrown
{
    [DoesNotReturn]
    internal static void ArgumentException(string message, string paramName)
    {
        throw new ArgumentException(message, paramName);
    }

    [DoesNotReturn]
    internal static void ArgumentNullException(string paramName)
    {
        throw new ArgumentNullException(paramName);
    }
}