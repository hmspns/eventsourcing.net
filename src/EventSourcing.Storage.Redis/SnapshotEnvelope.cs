using System.Runtime.InteropServices;
using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Storage.Redis;

[StructLayout(LayoutKind.Auto)]
internal readonly struct SnapshotEnvelope
{
    internal byte[] State { get; init; }
    
    internal TypeMappingId TypeId { get; init; }
    
    internal long AggregateVersion { get; init; }
    
    internal bool IsEmpty { get; init; }

    internal static SnapshotEnvelope Empty => new SnapshotEnvelope()
    {
        IsEmpty = true
    };
}