using System;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Engine.Exceptions;

namespace EventSourcing.Net.Engine.Extensions;

internal static class EventPackageExtensions
{
    internal static IEventEnvelope ToEventEnvelope<TId>(this IEventPackage eventPackage, Func<string, object> parser)
    {
        if (eventPackage.Payload == null)
        {
            Thrown.InvalidOperationException($"Payload for event {eventPackage.EventId} is null");    
        }
        
        Type aggregateIdType = typeof(TId);
        TId aggregateId = (TId)parser(eventPackage.StreamName.ToString());
        if (aggregateId == null)
        {
            Thrown.InvalidOperationException($"AggregateId for event {eventPackage.EventId} is null");
        }
        
        Type payloadType = eventPackage.Payload.GetType();
        Type baseType = typeof(EventEnvelope<,>);
        Type genericType = baseType.MakeGenericType(aggregateIdType, payloadType);

        object? payloadEvent = Activator.CreateInstance(genericType);
        IInitializablePayloadEvent<TId>? initializer = payloadEvent as IInitializablePayloadEvent<TId>;
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
}