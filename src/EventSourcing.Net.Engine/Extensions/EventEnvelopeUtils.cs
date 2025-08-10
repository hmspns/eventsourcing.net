namespace EventSourcing.Net.Engine.Extensions;

using System;
using System.Collections.Concurrent;
using Abstractions.Contracts;
using Exceptions;

/// <summary>
/// Utils for IEventEnvelope.
/// </summary>
internal static class EventEnvelopeUtils
{
    private static readonly ConcurrentDictionary<Type, Type> _cache = new ConcurrentDictionary<Type, Type>();

    /// <summary>
    /// Get type of IEventEnvelope<TId, TPayload> for IEventEnvelope.
    /// </summary>
    /// <param name="envelope">Envelope.</param>
    /// <returns></returns>
    internal static Type GetEnvelopeTypedInterface(this IEventEnvelope envelope)
    {
        Type envelopeType = envelope.GetType();

        if (_cache.TryGetValue(envelopeType, out Type intrefaceType))
        {
            return intrefaceType;
        }
        
        Type genericInterfaceType = typeof(IEventEnvelope<,>);

        foreach (Type interfaceType in envelopeType.GetInterfaces())
        {
            if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == genericInterfaceType)
            {
                _cache.TryAdd(envelopeType, interfaceType);
                return interfaceType;
            }
        }
        
        Thrown.InvalidOperationException($"Type {envelope.GetType()} should implement interface IEventEnvelope<TId, TPayload>");
        return null; // exception will be thrown on the line above.
    }
}