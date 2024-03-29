using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using MassTransit.Mediator;

namespace EventSourcing.Net.Bus.MassTransit;

/// <inheritdoc />
public sealed class EventPublisherResolver : IResolveEventPublisher
{
    private readonly IMediator _mediator;

    public EventPublisherResolver(IMediator mediator)
    {
        _mediator = mediator;
    }

    public IEventPublisher Get(TenantId tenantId)
    {
        return new EventPublisher(_mediator, tenantId);
    }
}

/// <inheritdoc />
public sealed class EventPublisher : IEventPublisher
{
    private readonly IMediator _mediator;
        
    public EventPublisher(IMediator mediator, TenantId tenantId)
    {
        _mediator = mediator;
    }
        
    public async Task Publish(ICommandEnvelope commandEnvelope, IReadOnlyList<IEventEnvelope> events)
    {
        await _mediator.Publish<IEsCommandPublicationStarted>(new EsCommandPublicationStarted(CommandId: commandEnvelope.CommandId, SequenceId: commandEnvelope.SequenceId)).ConfigureAwait(false);
            
        foreach (IEventEnvelope @event in events)
        {
            await _mediator.Publish(@event).ConfigureAwait(false);
        }

        await _mediator.Publish<IEsCommandPublicationCompleted>(new EsCommandPublicationCompleted(CommandId: commandEnvelope.CommandId, SequenceId: commandEnvelope.SequenceId)).ConfigureAwait(false);
    }
}