using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Net.Abstractions.Identities;

namespace EventSourcing.Net.Abstractions.Contracts;

using System;

/// <summary>
/// Command bus.
/// </summary>
public interface IEventSourcingCommandBus
{
    /// <summary>
    /// Send message to bus.
    /// </summary>
    /// <param name="tenantId">Tenant id.</param>
    /// <param name="principalId">Principal id.</param>
    /// <param name="source">Command source.</param>
    /// <param name="aggregateId">Aggregate id.</param>
    /// <param name="commandPayload">Command data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TId">Aggregate id type.</typeparam>
    /// <typeparam name="TPayload">Command payload type.</typeparam>
    /// <returns>Command execution result.</returns>
    /// <remarks>Command source is the place where command was sent.</remarks>
    Task<ICommandExecutionResult<TId>> Send<TId, TPayload>(TenantId tenantId, PrincipalId principalId, string source, 
        TId aggregateId, TPayload commandPayload, 
        CancellationToken cancellationToken = default)
        where TPayload : ICommand;
    
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
    Task<ICommandExecutionResult<TId>> Send<TId, TPayload>(ICommandEnvelope<TId> commandEnvelope, CancellationToken cancellationToken = default)
        where TPayload : ICommand;
        
    /// <summary>
    /// Get publication awaiter to await publication result.
    /// </summary>
    IPublicationAwaiter PublicationAwaiter { get; }
}