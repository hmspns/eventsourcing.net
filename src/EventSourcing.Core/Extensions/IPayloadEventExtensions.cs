using System;
using System.ComponentModel;
using System.Reflection;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Core.Extensions;

internal static class IEventPackageExtensions
{
    // internal static IEventEnvelope ToEventEnvelope<TId>(this IEventPackage eventPackage)
    // {
    //     Type aggregateIdType = typeof(TId);
    //     TId aggregateId = ParseId<TId>(eventPackage.StreamName, aggregateIdType);
    //     Type payloadType = eventPackage.Payload.GetType();
    //     Type baseType = typeof(EventEnvelope<,>);
    //     Type genericType = baseType.MakeGenericType(aggregateIdType, payloadType);
    //
    //     object payloadEvent = Activator.CreateInstance(genericType);
    //     IInitializablePayloadEvent initializer = (IInitializablePayloadEvent)payloadEvent;
    //     initializer.Payload = eventPackage.Payload;
    //     initializer.Timestamp = eventPackage.Timestamp;
    //     initializer.Version = eventPackage.StreamPosition;
    //     initializer.AggregateId = aggregateId;
    //     initializer.CommandId = eventPackage.CommandId;
    //     initializer.EventId = eventPackage.EventId;
    //     initializer.PrincipalId = eventPackage.PrincipalId;
    //     initializer.SequenceId = eventPackage.SequenceId;
    //     initializer.TenantId = eventPackage.TenantId;
    //         
    //     return (IEventEnvelope)payloadEvent;
    // }

    internal static IEventEnvelope ToEventEnvelope<TId>(this IEventPackage eventPackage)
    {
        Type aggregateIdType = typeof(TId);
        TId aggregateId = ParseId<TId>(eventPackage.StreamName, aggregateIdType);
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

    private static TId ParseId<TId>(StreamId raw, Type idType)
    {
        TypeConverterAttribute attribute = idType.GetCustomAttribute<TypeConverterAttribute>();
        TypeConverter converter = null;
        if (attribute != null)
        {
            Type converterType = Type.GetType(attribute.ConverterTypeName);
            converter = (TypeConverter)Activator.CreateInstance(converterType);
        }
        else
        {
            converter = TypeDescriptor.GetConverter(idType);
        }

        if (converter == null)
        {
            throw new InvalidOperationException("Couldn't find type converter for ");
        }

        if (converter.CanConvertFrom(typeof(string)))
        {
            return (TId)converter.ConvertFromString(raw.ToString());
        }

        return default;
    }
}