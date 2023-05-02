using System;
using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Net.Abstractions;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using MassTransit;
using MassTransit.Mediator;

namespace EventSourcing.Bus.MassTransit;

/// <inheritdoc />
public sealed class EventSourcingCommandBus : IEventSourcingCommandBus
{
    private readonly IMediator _mediator;

    public EventSourcingCommandBus(IMediator mediator, IPublicationAwaiter publicationAwaiter)
    {
        _mediator = mediator;
        PublicationAwaiter = publicationAwaiter;
    }

    public Task<ICommandExecutionResult<TId>> Send<TId, TPayload>(
        TId id,
        TPayload command,
        CancellationToken cancellationToken = default)
        where TPayload : ICommand
    {
        return Send(TenantId.Empty, PrincipalId.Empty, string.Empty, id, command, cancellationToken);
    }

    public async Task<ICommandExecutionResult<TId>> Send<TId, TPayload>(TenantId tenantId, PrincipalId principalId, string source,
        TId aggregateId, TPayload commandPayload,
        CancellationToken cancellationToken = default)
        where TPayload : ICommand
    {
        ICommandEnvelope<TId, TPayload> command = new CommandEnvelope<TId, TPayload>()
        {
            Payload = commandPayload,
            Timestamp = DateTime.UtcNow,
            AggregateId = aggregateId,
            CommandId = CommandId.New(),
            SequenceId = CommandSequenceId.New(),
            ParentCommandId = CommandId.Empty,
            TenantId = tenantId,
            Source = source,
            PrincipalId = principalId
        };

        IRequestClient<ICommandEnvelope<TId, TPayload>> requestClient = _mediator.CreateRequestClient<ICommandEnvelope<TId, TPayload>>();
        Response<ICommandExecutionResult<TId>> response = await requestClient.GetResponse<ICommandExecutionResult<TId>>(command, cancellationToken);
        return response.Message;
    }

    public IPublicationAwaiter PublicationAwaiter { get; }
}