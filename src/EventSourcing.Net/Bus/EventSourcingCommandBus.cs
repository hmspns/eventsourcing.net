using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Engine;
using EventSourcing.Net.Engine.Exceptions;
using EventSourcing.Net.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net;

/// <summary>
/// Built in command bus.
/// </summary>
public sealed class EventSourcingCommandBus : IEventSourcingCommandBus
{
    /// <inheritdoc />
    public IPublicationAwaiter PublicationAwaiter => _publicationAwaiter;

    private readonly IReadOnlyDictionary<Type, CommandHandlerActivation> _handlers;
    private readonly IServiceProvider _provider;
    // cached boxed CancellationToken.None to prevent boxing in case when cancellation token wasn't pass.
    private readonly object _boxedCancellationTokenNone = CancellationToken.None; 
    private readonly InMemoryPublicationAwaiter _publicationAwaiter = new();

    internal EventSourcingCommandBus(IServiceProvider provider, Dictionary<Type, CommandHandlerActivation> handlers)
    {
        _provider = provider;
        _handlers = handlers;
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
    /// <remarks>
    /// It's important to have a good performance use a specific type of TPayload instead of general ICommand.
    /// When ICommand passed as TPayload bus has to use reflection to find the proper handler and create command envelope.
    /// When a specific type passed as TPayload reflection not needed.
    /// </remarks>
    public Task<ICommandExecutionResult<TId>>? Send<TId, TPayload>(TenantId tenantId, PrincipalId principalId, string source,
        TId aggregateId, TPayload commandPayload, CancellationToken cancellationToken = default) where TPayload : ICommand
    {
        (ICommandEnvelope<TId> command, CommandHandlerActivation activator) data;
        if (typeof(TPayload) != typeof(ICommand))
        {
            data = GetDataFast(tenantId, principalId, source, aggregateId, commandPayload);
        }
        else
        {
            data = GetDataSlow(tenantId, principalId, source, aggregateId, commandPayload);
        }

        (ICommandEnvelope<TId> command, CommandHandlerActivation activator) = data;

        ICommandHandler instance = (ICommandHandler)ActivatorUtilities.GetServiceOrCreateInstance(_provider, activator.Type);
        instance.Engine = _provider.GetRequiredService<IEventSourcingEngine>();
        
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

    private (ICommandEnvelope<TId>, CommandHandlerActivation) GetDataFast<TId, TPayload>(
        TenantId tenantId,
        PrincipalId principalId,
        string source,
        TId aggregateId,
        TPayload commandPayload) where TPayload : ICommand
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
            Thrown.InvalidOperationException($"Handler for type {command.GetType().ToString()} not registered");
        }

        return (command, activator);
    }

    /// <summary>
    /// Create command envelope in case when TPayload is ICommand. We have to use reflection to create proper envelop.
    /// </summary>
    /// <param name="tenantId">Tenant id.</param>
    /// <param name="principalId">Principal id.</param>
    /// <param name="source">Command source.</param>
    /// <param name="aggregateId">Aggregate id.</param>
    /// <param name="commandPayload">Command payload.</param>
    /// <typeparam name="TId">Type of aggregate id.</typeparam>
    /// <returns></returns>
    private (ICommandEnvelope<TId>, CommandHandlerActivation) GetDataSlow<TId>(
        TenantId tenantId,
        PrincipalId principalId,
        string source,
        TId aggregateId,
        ICommand commandPayload)
    {
        Type envelopeType = typeof(CommandEnvelope<,>).MakeGenericType(typeof(TId), commandPayload.GetType());
        object command = Activator.CreateInstance(envelopeType)!;

        IInitializableCommandEnvelope initializable = (IInitializableCommandEnvelope)command;
        initializable.Payload = commandPayload;
        initializable.Timestamp = DateTime.UtcNow;
        initializable.AggregateId = aggregateId;
        initializable.CommandId = CommandId.New();
        initializable.SequenceId = CommandSequenceId.New();
        initializable.ParentCommandId = CommandId.Empty;
        initializable.TenantId = tenantId;
        initializable.Source = source;
        initializable.PrincipalId = principalId;

        Type handlerType = typeof(ICommandEnvelope<,>).MakeGenericType(typeof(TId), commandPayload.GetType());

        if(!_handlers.TryGetValue(handlerType, out CommandHandlerActivation activator))
        {
            Thrown.InvalidOperationException($"Handler for type {command.GetType().ToString()} not registered");
        }

        return ((ICommandEnvelope<TId>)command, activator);
    }
}