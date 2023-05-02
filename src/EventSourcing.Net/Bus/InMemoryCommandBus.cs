using System.Collections.ObjectModel;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Core;
using EventSourcing.Net.Core.Exceptions;
using EventSourcing.Net.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net;

public sealed class InMemoryCommandBus : IEventSourcingCommandBus
{
    public IPublicationAwaiter PublicationAwaiter => _publicationAwaiter;

    private readonly IReadOnlyDictionary<Type, CommandHandlerActivation> _handlers;
    private readonly IServiceProvider _provider;
    // cached boxed CancellationToken.None to prevent boxing in case when cancellation token wasn't pass.
    private readonly object _boxedCancellationTokenNone = CancellationToken.None; 
    private readonly InMemoryPublicationAwaiter _publicationAwaiter = new();

    internal InMemoryCommandBus(IServiceProvider provider, Dictionary<Type, CommandHandlerActivation> handlers)
    {
        _provider = provider;
        _handlers = handlers;
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
    
    private (ICommandEnvelope<TId>, CommandHandlerActivation) GetDataSlow<TId>(
        TenantId tenantId,
        PrincipalId principalId,
        string source,
        TId aggregateId,
        ICommand commandPayload)
    {
        Type envelopeType = typeof(CommandEnvelope<,>).MakeGenericType(typeof(TId), commandPayload.GetType());
        object command = Activator.CreateInstance(envelopeType)!;
        envelopeType.GetProperty(nameof(ICommandEnvelope.Payload))!.SetValue(command, commandPayload);
        envelopeType.GetProperty(nameof(ICommandEnvelope.Timestamp))!.SetValue(command, DateTime.UtcNow);
        envelopeType.GetProperty(nameof(ICommandEnvelope.AggregateId))!.SetValue(command, aggregateId);
        envelopeType.GetProperty(nameof(ICommandEnvelope.CommandId))!.SetValue(command, CommandId.New());
        envelopeType.GetProperty(nameof(ICommandEnvelope.SequenceId))!.SetValue(command, CommandSequenceId.New());
        envelopeType.GetProperty(nameof(ICommandEnvelope.ParentCommandId))!.SetValue(command, CommandId.Empty);
        envelopeType.GetProperty(nameof(ICommandEnvelope.TenantId))!.SetValue(command, tenantId);
        envelopeType.GetProperty(nameof(ICommandEnvelope.Source))!.SetValue(command, source);
        envelopeType.GetProperty(nameof(ICommandEnvelope.PrincipalId))!.SetValue(command, principalId);

        Type handlerType = typeof(ICommandEnvelope<,>).MakeGenericType(typeof(TId), commandPayload.GetType());

        if(!_handlers.TryGetValue(handlerType, out CommandHandlerActivation activator))
        {
            Thrown.InvalidOperationException($"Handler for type {command.GetType().ToString()} not registered");
        }

        return ((ICommandEnvelope<TId>)command, activator);
    }
}