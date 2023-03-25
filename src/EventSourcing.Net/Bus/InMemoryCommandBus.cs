using System.Collections.ObjectModel;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Net.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net;

public sealed class InMemoryCommandBus : IEventSourcingCommandBus
{
    public IPublicationAwaiter PublicationAwaiter => _publicationAwaiter;

    private readonly ReadOnlyDictionary<Type, CommandHandlerActivation> _handlers;
    private readonly IServiceProvider _provider;
    // cached boxed CancellationToken.None to prevent boxing in case when cancellation token wasn't pass.
    private readonly object _boxedCancellationTokenNone = CancellationToken.None; 
    private readonly InMemoryPublicationAwaiter _publicationAwaiter = new();

    internal InMemoryCommandBus(IServiceProvider provider, Dictionary<Type, CommandHandlerActivation> handlers)
    {
        _provider = provider;
        _handlers = new ReadOnlyDictionary<Type, CommandHandlerActivation>(handlers);
    }

    /// <summary>
    /// Send command to handler.
    /// </summary>
    /// <param name="id">Aggregate id.</param>
    /// <param name="command">Command payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TId">Type of aggregate id.</typeparam>
    /// <typeparam name="TPayload">Type of command payload.</typeparam>
    /// <returns>Result of command execution.</returns>
    /// <exception cref="InvalidOperationException">Handler not registered.</exception>
    public Task<ICommandExecutionResult<TId>>? Send<TId, TPayload>(TId id, TPayload command, CancellationToken cancellationToken = default) where TPayload : ICommand
    {
        return Send(TenantId.Empty, PrincipalId.Empty, string.Empty, id, command, cancellationToken);
    }

    /// <summary>
    /// Send command to handler.
    /// </summary>
    /// <param name="tenantId">Tenant id.</param>
    /// <param name="principalId">Id of principal.</param>
    /// <param name="source">Command source.</param>
    /// <param name="aggregateId">Aggregate id.</param>
    /// <param name="commandPayload">Command payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TId">Type of aggregate id.</typeparam>
    /// <typeparam name="TPayload">Type of command payload.</typeparam>
    /// <returns>Result of command execution.</returns>
    /// <exception cref="InvalidOperationException">Handler not registered.</exception>
    public Task<ICommandExecutionResult<TId>>? Send<TId, TPayload>(TenantId tenantId, PrincipalId principalId, string source,
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

        if(!_handlers.TryGetValue(typeof(ICommandEnvelope<TId, TPayload>), out CommandHandlerActivation activator))
        {
            throw new InvalidOperationException($"Handler for type {command.GetType()} not registered");
        }

        object instance = ActivatorUtilities.GetServiceOrCreateInstance(_provider, activator.Type);
        
        object? result;
        if (activator.UseCancellation)
        {
            object objCancellationToken = cancellationToken == CancellationToken.None
                ? _boxedCancellationTokenNone // to avoid boxing
                : cancellationToken; // real token with value
            result = activator.Method.Invoke(instance,new object?[]{ command, objCancellationToken});
        }
        else
        {
            result = activator.Method.Invoke(instance, new object?[] { command });
        }

        return result as Task<ICommandExecutionResult<TId>>;
    }
}