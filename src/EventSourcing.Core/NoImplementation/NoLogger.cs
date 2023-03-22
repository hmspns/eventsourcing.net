using System;
using System.Collections.Generic;
using EventSourcing.Abstractions;

namespace EventSourcing.Core.NoImplementation;

public class NoLogger : ILogger
{
    public ILogger WithProperty(string name, object value)
    {
        return this;
    }

    public void Event(string eventType, Dictionary<string, object> dictionary)
    {
    }

    public void LogMessage(LogLevel level, string message, params object[] args)
    {
    }

    public void LogExceptionMessage(Exception ex, LogLevel level, string message, params object[] args)
    {
    }

    public ITimedOperation StartTimedOperation(LogLevel logLevel, string message, params object[] args)
    {
        return new NoOperation();
    }
}

public sealed class NoLoggerFactory : ILoggerFactory
{
    public ILogger CreateLogger<T>()
    {
        return new NoLogger();
    }

    public ILoggerFactory WithProperty(string name, object? value)
    {
        return this;
    }
}

public class NoOperation : ITimedOperation
{
    public void Dispose()
    {
        
    }

    public void Complete()
    {
    }
}