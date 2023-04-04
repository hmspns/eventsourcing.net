using EventSourcing.Abstractions.Types;

namespace EventSourcing.Storage.Postgres;

public interface IPgCommandTextProvider
{
    /// <summary>
    /// Get command text to append event.
    /// </summary>
    string InsertEvent { get; }
    /// <summary>
    /// Get command text to append command.
    /// </summary>
    string InsertCommand { get; }
    /// <summary>
    /// Get command text fetch events from the stream.
    /// </summary>
    string SelectStreamData { get; }
    /// <summary>
    /// Get command to select version of the stream.
    /// </summary>
    string SelectStreamVersion { get; }
    /// <summary>
    /// Get commands to select count of event in the stream.
    /// </summary>
    string SelectEventCounts { get; }
    /// <summary>
    /// Get commands to check whether storage exists.
    /// </summary>
    string SelectStorageExists { get; }
    /// <summary>
    /// Get command to create storage for events.
    /// </summary>
    string CreateStorage { get; }
    /// <summary>
    /// Get command to select streams ids by pattern.
    /// </summary>
    string SelectStreamIdsByPattern { get; }

    /// <summary>
    /// Build command to read data by custom filter.
    /// </summary>
    /// <param name="readOptions">Options how to read data.</param>
    /// <returns>Command text.</returns>
    string BuildReadAllStreamsCommandText(StreamReadOptions readOptions);
}