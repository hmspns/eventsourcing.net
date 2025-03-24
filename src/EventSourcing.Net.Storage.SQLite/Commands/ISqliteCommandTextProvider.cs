namespace EventSourcing.Net.Storage.SQLite.Commands;

using Abstractions.Types;

public interface ISqliteCommandTextProvider
{
    string InsertEvent { get; }
    string InsertCommand { get; }
    string SelectStreamData { get; }
    string SelectStreamVersion { get; }
    string SelectEventCounts { get; }
    string SelectStorageExists { get; }
    string CreateDataStorage { get; }
    string SelectStreamIdsByPattern { get; }
    string CreateMappingsStorage { get; }
    string SelectTypeMappings { get; }
    string InsertTypeMapping { get; }
    string BuildReadAllStreamsCommandText(StreamReadOptions readOptions);
}