using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Abstractions;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Core.InMemory;

public sealed class InMemoryCommandBus : IEventSourcingCommandBus
{
    public IPublicationAwaiter PublicationAwaiter { get; }

    private static Dictionary<Type, CommandHandlerActivation> _handlers = new();
    private IServiceProvider _provider;

    internal static void Initialize(Dictionary<Type, CommandHandlerActivation> handlers)
    {
        _handlers = handlers;
    }
    
    internal static void Initialize(Dictionary<Type, Func<object, ICommandEnvelope, CancellationToken, object>> handlers)
    {
        //_handlers = handlers;
    }

    public InMemoryCommandBus(IServiceProvider provider)
    {
        _provider = provider;
    }

    public Task<ICommandExecutionResult<TId>> Send<TId, TPayload>(TId id, TPayload command, CancellationToken cancellationToken = default) where TPayload : ICommand
    {
        return Send(TenantId.Empty, PrincipalId.Empty, string.Empty, id, command, cancellationToken);
    }

    public Task<ICommandExecutionResult<TId>> Send<TId, TPayload>(TenantId tenantId, PrincipalId principalId, string source,
        TId aggregateId, TPayload commandPayload, CancellationToken cancellationToken = default) where TPayload : ICommand
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

        if(!_handlers.TryGetValue(command.GetType(), out var activator))
        {
            throw new InvalidOperationException($"Handler for type {command.GetType()} not registered");
        }

        return (Task<ICommandExecutionResult<TId>>)activator(command, cancellationToken);
    }
}

internal sealed class CommandHandlerActivation
{
    internal MethodInfo Method { get; init; }
    
    internal bool UseCancellation { get; init; }
}