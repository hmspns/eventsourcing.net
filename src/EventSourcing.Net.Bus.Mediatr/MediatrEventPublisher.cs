using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Engine;
using EventSourcing.Net.Engine.Exceptions;
using MediatR;

namespace EventSourcing.Net.Bus.Mediatr;

/// <summary>
/// Resolver for Mediatr publishing.
/// </summary>
public sealed class MediatrEventPublisherResolver : IResolveEventPublisher
{
    private readonly IPublisher _publisher;

    public MediatrEventPublisherResolver(IPublisher publisher)
    {
        _publisher = publisher;
    }

    /// <summary>
    /// Return tenant specific event publisher.
    /// </summary>
    /// <param name="tenantId">Tenant id.</param>
    /// <returns>Tenant specific event publisher.</returns>
    public IEventPublisher Get(TenantId tenantId)
    {
        return new MediatrEventPublisher(_publisher);
    }
}

/// <summary>
/// Events publisher through Mediatr bus.
/// </summary>
public sealed class MediatrEventPublisher : IEventPublisher
{
    private readonly IPublisher _publisher;

    public MediatrEventPublisher(IPublisher publisher)
    {
        _publisher = publisher;
    }

    /// <summary>
    /// Publish event.
    /// </summary>
    /// <param name="commandEnvelope">Command data.</param>
    /// <param name="events">Events to be published.</param>
    public async Task Publish(ICommandEnvelope commandEnvelope, IReadOnlyList<IEventEnvelope> events)
    {
        foreach (IEventEnvelope envelope in events)
        {
            IEventEnvelope mediatrEnvelope = GetMediatrEnvelope(envelope);
            await _publisher.Publish(mediatrEnvelope);
        }
    }

    private static IEventEnvelope GetMediatrEnvelope(IEventEnvelope originalEnvelope)
    {
        Type t = originalEnvelope.GetEnvelopeTypedInterface();
        if (t.GenericTypeArguments.Length != 2)
        {
            Thrown.ArgumentException("Implementation of IEventEnvelope should also implement IEventEnvelope<TId, TPayload>", nameof(originalEnvelope));
        }
        
        Type idType = t.GenericTypeArguments[0];
        Type payloadType = t.GenericTypeArguments[1];

        Type genericType = typeof(MediatrEventEnvelope<,>);
        Type concreteType = genericType.MakeGenericType(idType, payloadType);

        object envelope = Activator.CreateInstance(concreteType);

        if (envelope == null)
        {
            Thrown.InvalidOperationException("Couldn't create instance for type " + concreteType.ToString());
        }

        IInitializableEventEnvelope initializable = (IInitializableEventEnvelope)envelope;

        initializable.AggregateId = originalEnvelope.AggregateId;
        initializable.Payload = originalEnvelope.Payload;
        initializable.CommandId = originalEnvelope.CommandId;
        initializable.Timestamp = originalEnvelope.Timestamp;
        initializable.TenantId = originalEnvelope.TenantId;
        initializable.EventId = originalEnvelope.EventId;
        initializable.PrincipalId = originalEnvelope.PrincipalId;
        initializable.SequenceId = originalEnvelope.SequenceId;

        return (IEventEnvelope)envelope;
    }
}