using System.Linq;

namespace EventSourcing.Core
{
    /// <summary>
    /// Iteration stream direction.
    /// </summary>
    public enum StreamReadDirection
    {
        /// <summary>
        /// From old to new.
        /// </summary>
        Forward,
        /// <summary>
        /// From new to old.
        /// </summary>
        Backward
    }

    /// <summary>
    /// Filter type.
    /// </summary>
    public enum AggregateStreamFilterType
    {
        /// <summary>
        /// Include by params.
        /// </summary>
        Include,
        /// <summary>
        /// Exclude by params.
        /// </summary>
        Except
    }
    
    /// <summary>
    /// Stream read options.
    /// </summary>
    public enum StreamReadVolume
    {
        /// <summary>
        /// Read metadata only.
        /// </summary>
        Meta,
        /// <summary>
        /// Read data only.
        /// </summary>
        Data,
        /// <summary>
        /// Read metadata and data.
        /// </summary>
        MetaAndData,
    }

    /// <summary>
    /// Storage position type.
    /// </summary>
    public enum StoragePositionType
    {
        /// <summary>
        /// Across all streams.
        /// </summary>
        GlobalPosition,
        /// <summary>
        /// Across specific stream.
        /// </summary>
        StreamPosition
    }

    /// <summary>
    /// Stream options.
    /// </summary>
    public record StreamReadOptions
    {
        /// <summary>
        /// Get position for last event.
        /// </summary>
        public static readonly StreamReadOptions LastEvent = new StreamReadOptions(0, 1, StreamReadDirection.Backward);

        /// <summary>
        /// Initialize 
        /// </summary>
        /// <param name="from">Start position.</param>
        /// <param name="to">End position.</param>
        /// <param name="direction">Direction of reading.</param>
        /// <param name="positionType">Type of storage position.</param>
        /// <param name="filterType">Filter type.</param>
        /// <param name="readVolume">Read volume.</param>
        /// <param name="prefixPattern">Optional prefix to filter data.</param>
        public StreamReadOptions(StreamPosition? @from = null, StreamPosition? to = null,
            StreamReadDirection? direction = null,
            StoragePositionType? positionType = null,
            AggregateStreamFilterType? filterType = null,
            StreamReadVolume? readVolume = null, 
            string[]? prefixPattern = null)
        {
            From = from ?? StreamPosition.Begin;
            To = to ?? StreamPosition.End;
            ReadDirection = direction ?? StreamReadDirection.Forward;
            StoragePositionType = positionType ?? StoragePositionType.GlobalPosition;
            FilterType = filterType ?? AggregateStreamFilterType.Include;
            PrefixPattern = prefixPattern;
            ReadingVolume = readVolume ?? StreamReadVolume.MetaAndData;

            if (PrefixPattern == null || !PrefixPattern.Any())
            {
                FilterType = AggregateStreamFilterType.Include;
            }
        }

        /// <summary>
        /// Get prefix pattern.
        /// </summary>
        public string[] PrefixPattern { get; init; }

        /// <summary>
        /// Get filter type.
        /// </summary>
        public AggregateStreamFilterType FilterType { get; init; }

        /// <summary>
        /// Get read direction.
        /// </summary>
        public StreamReadDirection ReadDirection { get; init; }
        
        /// <summary>
        /// Get the storage position.
        /// </summary>
        public StoragePositionType StoragePositionType { get; init; }

        /// <summary>
        /// Get reading volume.
        /// </summary>
        public StreamReadVolume ReadingVolume { get; init; }

        /// <summary>
        /// Get end position.
        /// </summary>
        public StreamPosition To { get; init; }

        /// <summary>
        /// Get start position.
        /// </summary>
        public StreamPosition From { get; init; }
    }
}