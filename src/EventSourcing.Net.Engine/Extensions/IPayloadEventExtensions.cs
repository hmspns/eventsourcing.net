using System;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Engine.Exceptions;

namespace EventSourcing.Net.Engine.Extensions;

using System.Collections.Concurrent;
using System.Collections.Generic;
using Abstractions.Types;

internal static class EventPackageExtensions
{
    private static readonly ConcurrentDictionary<CacheKey, Type> _typeCache = new ConcurrentDictionary<CacheKey, Type>();

    internal static IEventEnvelope ToEventEnvelope<TId>(this in EventPackage eventPackage, Func<string, object> parser)
    {
        if (eventPackage.Payload == null)
        {
            Thrown.InvalidOperationException($"Payload for event {eventPackage.EventId} is null");    
        }
        
        TId aggregateId = (TId)parser(eventPackage.StreamName.ToString());
        if (aggregateId == null)
        {
            Thrown.InvalidOperationException($"AggregateId for event {eventPackage.EventId} is null");
        }
        
        Type genericType = GetEnvelopeType<TId>(eventPackage.Payload.GetType());

        object? payloadEvent = Activator.CreateInstance(genericType);
        IInitializableEventEnvelope<TId>? initializer = payloadEvent as IInitializableEventEnvelope<TId>;
        if (payloadEvent == null || initializer == null)
        {
            Thrown.InvalidOperationException("Couldn't create instance for type " + genericType.FullName + " for event " + eventPackage.EventId);    
        }
        
        initializer.Payload = eventPackage.Payload;
        initializer.Timestamp = eventPackage.Timestamp;
        initializer.Version = eventPackage.StreamPosition;
        initializer.AggregateId = aggregateId;
        initializer.CommandId = eventPackage.CommandId;
        initializer.EventId = eventPackage.EventId;
        initializer.PrincipalId = eventPackage.PrincipalId;
        initializer.SequenceId = eventPackage.SequenceId;
        initializer.TenantId = eventPackage.TenantId;
            
        return (IEventEnvelope)payloadEvent;
    }

    private static Type GetEnvelopeType<TId>(Type payloadType)
    {
        Type idType = typeof(TId);
        CacheKey key = new CacheKey(idType, payloadType);
        
        if (_typeCache.TryGetValue(key, out Type? envelopeType))
        {
            return envelopeType;
        }
        
        Type aggregateIdType = idType;
        Type baseType = typeof(EventEnvelope<,>);
        Type genericType = baseType.MakeGenericType(aggregateIdType, payloadType);

        _typeCache.TryAdd(key, genericType);

        return genericType;
    }

    private readonly struct CacheKey : IEquatable<CacheKey>
    {
        private readonly Type _idType;
        private readonly Type _payloadType;

        public CacheKey(Type idType, Type payloadType)
        {
            _idType = idType;
            _payloadType = payloadType;
        }

        public bool Equals(CacheKey other)
        {
            return _idType == other._idType && _payloadType == other._payloadType;
        }

        public override bool Equals(object? obj)
        {
            return obj is CacheKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_idType, _payloadType);
        }
    }
}