using System.Runtime.CompilerServices;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;

namespace EventSourcing.Net;

/// <summary>
/// Extensions for command bus.
/// </summary>
public static class EventSourcingBusExtensions
{
    /// <summary>
    /// Send command to handler.
    /// </summary>
    /// <param name="bus">Bus to send the command.</param>
    /// <param name="id">Aggregate id.</param>
    /// <param name="command">Command payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TId">Type of aggregate id.</typeparam>
    /// <typeparam name="TPayload">Type of command payload.</typeparam>
    /// <returns>Result of command execution.</returns>
    /// <exception cref="InvalidOperationException">Handler not registered.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<ICommandExecutionResult<TId>> Send<TId, TPayload>(this IEventSourcingCommandBus bus, TId id, TPayload command, CancellationToken cancellationToken = default) where TPayload : ICommand
    {
        return bus.Send(TenantId.Empty, PrincipalId.Empty, string.Empty, id, command, cancellationToken);
    }

    /// <summary>
    /// Send command to handler.
    /// </summary>
    /// <param name="bus">Bus to send the command.</param>
    /// <param name="tenantId">Tenant id.</param>
    /// <param name="id">Aggregate id.</param>
    /// <param name="command">Command payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TId">Type of aggregate id.</typeparam>
    /// <typeparam name="TPayload">Type of command payload.</typeparam>
    /// <returns>Result of command execution.</returns>
    /// <exception cref="InvalidOperationException">Handler not registered.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<ICommandExecutionResult<TId>> Send<TId, TPayload>(this IEventSourcingCommandBus bus, TenantId tenantId, TId id, TPayload command, CancellationToken cancellationToken = default) where TPayload : ICommand
    {
        return bus.Send(tenantId, PrincipalId.Empty, string.Empty, id, command, cancellationToken);
    }

    /// <summary>
    /// Send command to handler.
    /// </summary>
    /// <param name="bus">Bus to send the command.</param>
    /// <param name="tenantId">Tenant id.</param>
    /// <param name="id">Aggregate id.</param>
    /// <param name="command">Command payload.</param>
    /// <param name="principalId">Id of the principal who call the command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TId">Type of aggregate id.</typeparam>
    /// <typeparam name="TPayload">Type of command payload.</typeparam>
    /// <returns>Result of command execution.</returns>
    /// <exception cref="InvalidOperationException">Handler not registered.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<ICommandExecutionResult<TId>> Send<TId, TPayload>(this IEventSourcingCommandBus bus, TenantId tenantId, PrincipalId principalId, TId id, TPayload command, CancellationToken cancellationToken = default) where TPayload : ICommand
    {
        return bus.Send(tenantId, principalId, string.Empty, id, command, cancellationToken);
    }

    /// <summary>
    /// Send command to handler.
    /// </summary>
    /// <param name="bus">Bus to send the command.</param>
    /// <param name="tenantId">Tenant id.</param>
    /// <param name="id">Aggregate id.</param>
    /// <param name="command">Command payload.</param>
    /// <param name="commandSource">Source that generated the command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TId">Type of aggregate id.</typeparam>
    /// <typeparam name="TPayload">Type of command payload.</typeparam>
    /// <returns>Result of command execution.</returns>
    /// <exception cref="InvalidOperationException">Handler not registered.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<ICommandExecutionResult<TId>> Send<TId, TPayload>(this IEventSourcingCommandBus bus, TenantId tenantId, string commandSource, TId id, TPayload command, CancellationToken cancellationToken = default) where TPayload : ICommand
    {
        return bus.Send(tenantId, PrincipalId.Empty, commandSource, id, command, cancellationToken);
    }

    /// <summary>
    /// Send command to handler.
    /// </summary>
    /// <param name="bus">Bus to send the command.</param>
    /// <param name="principalId">Id of the principal who call the command.</param>
    /// <param name="id">Aggregate id.</param>
    /// <param name="command">Command payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TId">Type of aggregate id.</typeparam>
    /// <typeparam name="TPayload">Type of command payload.</typeparam>
    /// <returns>Result of command execution.</returns>
    /// <exception cref="InvalidOperationException">Handler not registered.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<ICommandExecutionResult<TId>> Send<TId, TPayload>(this IEventSourcingCommandBus bus, PrincipalId principalId, TId id, TPayload command, CancellationToken cancellationToken = default) where TPayload : ICommand
    {
        return bus.Send(TenantId.Empty, principalId, string.Empty, id, command, cancellationToken);
    }
    
    /// <summary>
    /// Send command to handler.
    /// </summary>
    /// <param name="bus">Bus to send the command.</param>
    /// <param name="commandSource">Source that generated the command.</param>
    /// <param name="id">Aggregate id.</param>
    /// <param name="command">Command payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TId">Type of aggregate id.</typeparam>
    /// <typeparam name="TPayload">Type of command payload.</typeparam>
    /// <returns>Result of command execution.</returns>
    /// <exception cref="InvalidOperationException">Handler not registered.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<ICommandExecutionResult<TId>> Send<TId, TPayload>(this IEventSourcingCommandBus bus, string commandSource, TId id, TPayload command, CancellationToken cancellationToken = default) where TPayload : ICommand
    {
        return bus.Send(TenantId.Empty, PrincipalId.Empty, commandSource, id, command, cancellationToken);
    }

    /// <summary>
    /// Send command to handler.
    /// </summary>
    /// <param name="bus">Bus to send the command.</param>
    /// <param name="principalId">Id of the principal who call the command.</param>
    /// <param name="commandSource">Source that generated the command.</param>
    /// <param name="id">Aggregate id.</param>
    /// <param name="command">Command payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TId">Type of aggregate id.</typeparam>
    /// <typeparam name="TPayload">Type of command payload.</typeparam>
    /// <returns>Result of command execution.</returns>
    /// <exception cref="InvalidOperationException">Handler not registered.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<ICommandExecutionResult<TId>> Send<TId, TPayload>(this IEventSourcingCommandBus bus, PrincipalId principalId, string commandSource, TId id, TPayload command, CancellationToken cancellationToken = default) where TPayload : ICommand
    {
        return bus.Send(TenantId.Empty, principalId, commandSource, id, command, cancellationToken);
    }
}