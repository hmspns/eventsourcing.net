using System.Runtime.InteropServices;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Storage.Postgres;

[StructLayout(LayoutKind.Auto)]
public readonly struct InsertEventCommandArguments<TId>
{
    public IAppendDataPackage<TId> Data { get; init; }
    public IAppendEventPackage AppendPackage { get; init; }
    public TypeMappingId AggregateIdType { get; init; }
    public long Position { get; init; }
    public byte[] Payload { get; init; }
    public TypeMappingId PayloadType { get; init; }
    public string SchemaName { get; init; }
    public string EventsTableName { get; init; }
}