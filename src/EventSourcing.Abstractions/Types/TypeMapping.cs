using System.Runtime.InteropServices;
using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Abstractions.Types;

/// <summary>
/// Handle information about type mapping.
/// </summary>
/// <param name="Id">Type id.</param>
/// <param name="TypeName">Type name.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct TypeMapping(TypeMappingId Id, string TypeName);