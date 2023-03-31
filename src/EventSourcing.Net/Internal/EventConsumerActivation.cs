using System.Reflection;
using System.Runtime.InteropServices;

namespace EventSourcing.Net.Internal;

[StructLayout(LayoutKind.Auto)]
internal readonly struct EventConsumerActivation
{
    internal Type Type { get; init; }
    
    internal MethodInfo Method { get; init; }
}