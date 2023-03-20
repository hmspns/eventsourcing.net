using System;
using System.Collections.Generic;

namespace EventSourcing.Abstractions
{
    public enum LogLevel
    {
        Verbose = 0,
        Debug = 1,
        Information = 2,
        Warning = 3,
        Error = 4,
        Sensitive = 5
    }

    /// <summary>
    /// Abstract logger.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Create logger with custom property.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="value">Property value/.</param>
        /// <returns>New logger with predefined property.</returns>
        ILogger WithProperty(string name, object value);
        
        void Event(string eventType, Dictionary<string,object> dictionary);

        void LogMessage(LogLevel level, string message, params object[] args);

        void LogExceptionMessage(Exception ex, LogLevel level, string message, params object[] args);

        void Verbose(string messageTemplate) 
            => LogMessage(LogLevel.Verbose, messageTemplate);

        void Verbose(string messageTemplate, params object[] values) 
            => LogMessage(LogLevel.Verbose, messageTemplate, values);

        void VerboseSensitive(string messageTemplate, params object[] values)
            => LogMessage(LogLevel.Sensitive, messageTemplate, values);

        void Debug(string messageTemplate)
            => LogMessage(LogLevel.Debug, messageTemplate);
        void Debug(string messageTemplate, params object[] values)
            => LogMessage(LogLevel.Debug, messageTemplate, values);

        void Information(string messageTemplate) 
            => LogMessage(LogLevel.Information, messageTemplate);

        void Information(string messageTemplate, params object[] values)
            => LogMessage(LogLevel.Information, messageTemplate, values);

        void Warning(string messageTemplate, params object[] values)
            => LogMessage(LogLevel.Warning, messageTemplate, values);

        void Warning(Exception exception, string messageTemplate)
            => LogExceptionMessage(exception, LogLevel.Warning, messageTemplate);

        void Warning(Exception exception, string messageTemplate, params object[] values)
            => LogExceptionMessage(exception, LogLevel.Warning, messageTemplate, values);

        void Error(Exception exception, string messageTemplate)
            => LogExceptionMessage(exception, LogLevel.Error, messageTemplate);

        void Error(Exception exception, string messageTemplate, params object[] values)
            => LogExceptionMessage(exception, LogLevel.Error, messageTemplate, values);

        void Error(string messageTemplate, params object[] values)
            => LogMessage(LogLevel.Error, messageTemplate, values);
        
        /// <summary>
        /// Return measurement helper to measure time of operation.
        /// </summary>
        /// <param name="logLevel">Desired log level.</param>
        /// <param name="message">Message template.</param>
        /// <param name="args">Arguments for template.</param>
        ITimedOperation StartTimedOperation(LogLevel logLevel, string message, params object[] args);
    }
}