namespace EventSourcing.Abstractions
{
    /// <summary>
    /// Logger factory contract.
    /// </summary>
    public interface ILoggerFactory
    {
        /// <summary>
        /// Create new logger for specific type T.
        /// </summary>
        /// <typeparam name="T">Class type for logging.</typeparam>
        /// <returns>Logger specified for <paramref name="T"/>.</returns>
        ILogger CreateLogger<T>();

        /// <summary>
        /// Create new factory with custom property.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="value">Property value/.</param>
        /// <returns>New logger with predefined property.</returns>
        ILoggerFactory WithProperty(string name, object? value);
    }
}