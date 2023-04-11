using EventSourcing.Abstractions.Types;

namespace EventSourcing.Storage.Postgres;

public interface IPgCommandTextProvider
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