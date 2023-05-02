using System.Runtime.InteropServices;
using EventSourcing.Net.Abstractions.Identities;

namespace EventSourcing.Storage.Redis;

[StructLayout(LayoutKind.Auto)]
internal readonly struct SnapshotEnvelope
{
    internal TypeMappingId TypeId { get; init; }
    
    internal long AggregateVersion { get; init; }

    internal Memory<byte> State { get; init; }
}