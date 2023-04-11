using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Abstractions.Contracts;

/// <summary>
/// Command bus.
/// </summary>
public interface IEventSourcingCommandBus
{
    /// <summary>
    /// Send the command.
    /// </summary>
    /// <param name="id">Aggregate id.</param>
    /// <param name="command">Command.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result of executed command.</returns>
    Task<ICommandExecutionResult<TId>> Send<TId, TPayload>(TId id, TPayload command,
        CancellationToken cancellationToken = default) where TPayload : ICommand;
        
    /// <summary>
    /// Send message to bus.
    /// </summary>
    /// <param name="tenantId">Tenant id.</param>
    /// <param name="principalId">Principal id.</param>
    /// <param name="source">Command sounrce.</param>
    /// <param name="aggregateId">Aggregate id.</param>
    /// <param name="commandPayload">Command data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TId">Aggregate id type.</typeparam>
    /// <typeparam name="TPayload">Command payload type.</typeparam>
    /// <returns>Command execution result.</returns>
    Task<ICommandExecutionResult<TId>> Send<TId, TPayload>(TenantId tenantId, PrincipalId principalId, string source, 
        TId aggregateId, TPayload commandPayload, 
        CancellationToken cancellationToken = default)
        where TPayload : ICommand;
        
    /// <summary>
    /// Get publication awaiter to await publication result.
    /// </summary>
    IPublicationAwaiter PublicationAwaiter { get; }
}