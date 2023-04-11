using System.Runtime.InteropServices;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;

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