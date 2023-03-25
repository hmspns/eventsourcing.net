using System.Reflection;
using System.Runtime.InteropServices;

namespace EventSourcing.Net.Internal;

[StructLayout(LayoutKind.Auto)]
internal readonly struct EventConsumerActivation
{
    internal Type Type => Method.DeclaringType;
    
    internal MethodInfo Method { get; init; }
}