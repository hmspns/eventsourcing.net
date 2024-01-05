using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Engine;
using EventSourcing.Net.Engine.Exceptions;
using EventSourcing.Net.Internal;
using Microsoft.Extensions.DependencyInjection;

#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

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
#if NET8_0_OR_GREATER
        _handlers = handlers.ToFrozenDictionary();
#else
        _handlers = handlers;
#endif
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
    /// Command source is the place where command was sent.
    /// 
    /// It's important to have a good performance use a specific type of TPayload instead of general ICommand.
    /// When ICommand passed as TPayload bus has to use reflection to find the proper handler and create command envelope.
    /// When a specific type passed as TPayload reflection not needed.
    /// </remarks>
    public Task<ICommandExecutionResult<TId>> Send<TId, TPayload>(TenantId tenantId, PrincipalId principalId, string source,
        TId aggregateId, TPayload commandPayload, CancellationToken cancellationToken = default) where TPayload : ICommand
    {
        ICommandEnvelope<TId> command = CommandEnvelopeBuilder.ToEnvelope(tenantId, principalId, source, aggregateId, commandPayload);
        return Send<TId, TPayload>(command, cancellationToken);
    }

    /// <summary>
    /// Send command to handler.
    /// </summary>
    /// <param name="commandEnvelope">Command envelope that will be sent to handler.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <typeparam name="TId">Type of aggregate id.</typeparam>
    /// <typeparam name="TPayload">Type of command payload.</typeparam>
    /// <returns>Result of command execution.</returns>
    /// <exception cref="InvalidOperationException">Handler not registered.</exception>
    /// <remarks>
    /// Command source is the place where command was sent.
    /// 
    /// It's important to have a good performance use a specific type of TPayload instead of general ICommand.
    /// When ICommand passed as TPayload bus has to use reflection to find the proper handler and create command envelope.
    /// When a specific type passed as TPayload reflection not needed.
    /// </remarks>
    public Task<ICommandExecutionResult<TId>> Send<TId, TPayload>(ICommandEnvelope<TId> commandEnvelope, CancellationToken cancellationToken = default)
        where TPayload : ICommand
    {
        CommandHandlerActivation activator = GetActivator<TId, TPayload>((TPayload)commandEnvelope.Payload);

        ICommandHandler instance = (ICommandHandler)ActivatorUtilities.GetServiceOrCreateInstance(_provider, activator.Type);
        instance.Engine = _provider.GetRequiredService<IEventSourcingEngine>();
        
        object? result;
        if (activator.UseCancellation)
        {
            object objCancellationToken = cancellationToken == CancellationToken.None
                ? _boxedCancellationTokenNone // to avoid boxing
                : cancellationToken; // real token with value
            result = activator.Method.Invoke(instance,new object?[]{ commandEnvelope, objCancellationToken});
        }
        else
        {
            result = activator.Method.Invoke(instance, new object?[] { commandEnvelope });
        }

        Task<ICommandExecutionResult<TId>>? task = result as Task<ICommandExecutionResult<TId>>;
        if (task == null)
        {
            Thrown.InvalidOperationException("Command handler method must return Task<ICommandExecutionResult<TId>>");
        }

        return task;
    }

    private CommandHandlerActivation GetActivator<TId, TPayload>(TPayload payload) where TPayload : ICommand
    {
        Type handlerType;
        if (typeof(TPayload) != typeof(ICommand))
        {
            handlerType = typeof(ICommandEnvelope<TId, TPayload>);
        }
        else
        {
            handlerType = typeof(ICommandEnvelope<,>).MakeGenericType(typeof(TId), payload.GetType());
        }

        if(!_handlers.TryGetValue(handlerType, out CommandHandlerActivation activator))
        {
            Thrown.InvalidOperationException($"Handler for type {handlerType.ToString()} not registered");
        }

        return activator;
    }
}