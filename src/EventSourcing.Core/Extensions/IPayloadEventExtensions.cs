using System;
using EventSourcing.Abstractions.Contracts;

namespace EventSourcing.Core.Extensions;

internal static class IEventPackageExtensions
{
    internal static IEventEnvelope ToEventEnvelope(this IEventPackage eventPackage)
    {
        object aggregateId = eventPackage.StreamName.ToString();
        Type aggregateIdType = aggregateId.GetType();
        Type payloadType = eventPackage.Payload.GetType();
        Type baseType = typeof(EventEnvelope<,>);
        Type genericType = baseType.MakeGenericType(aggregateIdType, payloadType);

        object payloadEvent = Activator.CreateInstance(genericType);
        IInitializablePayloadEvent initializer = (IInitializablePayloadEvent)payloadEvent;
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