using System;

namespace EventSourcing.Core.Exceptions;

/// <summary>
/// Appender exception. Means that current version of events stream not equal to expected version.
/// </summary>
public class AppendOnlyStoreConcurrencyException : Exception
{
    /// <summary>
    /// Expected stream version.
    /// </summary>
    public long ExpectedStreamVersion { get; }
    /// <summary>
    /// Actual stream version.
    /// </summary>
    public long ActualStreamVersion { get; }
    /// <summary>
    /// Name of the stream.
    /// </summary>
    public string StreamName { get; }

    public AppendOnlyStoreConcurrencyException(long expectedVersion, long actualVersion, string name)
        : base($"Appender expected that version of stream '{name}' will be {expectedVersion}. But stream has version {actualVersion}")
    {
        StreamName = name;
        ExpectedStreamVersion = expectedVersion;
        ActualStreamVersion = actualVersion;
    }
        
    public AppendOnlyStoreConcurrencyException(Exception baseException, long expectedVersion, long actualVersion, string name)
        : base($"Appender expected that version of stream '{name}' will be {expectedVersion}. But stream has version {actualVersion}", baseException)
    {
        StreamName = name;
        ExpectedStreamVersion = expectedVersion;
        ActualStreamVersion = actualVersion;
    }
}