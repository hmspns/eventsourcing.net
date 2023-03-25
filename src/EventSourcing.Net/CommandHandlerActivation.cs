using System.Reflection;
using System.Runtime.InteropServices;

namespace EventSourcing.Net;

[StructLayout(LayoutKind.Auto)]
internal readonly struct CommandHandlerActivation
{
    internal Type Type => Method.DeclaringType;
    
    internal MethodInfo Method { get; init; }
    
    internal bool UseCancellation { get; init; }
}