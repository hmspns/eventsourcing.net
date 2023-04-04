namespace EventSourcing.Storage.Postgres;

public interface IPgCommandTextProvider
{
    string InsertEvent { get; }
    string InsertCommand { get; }
    string SelectStreamData { get; }
    string SelectStreamVersion { get; }
    string SelectEventCounts { get; }
}