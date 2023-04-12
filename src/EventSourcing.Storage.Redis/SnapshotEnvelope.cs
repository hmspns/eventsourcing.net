﻿using System.Runtime.InteropServices;
using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Storage.Redis;

[StructLayout(LayoutKind.Auto)]
internal readonly struct SnapshotEnvelope
{
    internal byte[] State { get; init; }

    internal TypeMappingId TypeId { get; init; }
    
    internal long AggregateVersion { get; init; }

    internal Memory<byte> MemoryState { get; init; }

}