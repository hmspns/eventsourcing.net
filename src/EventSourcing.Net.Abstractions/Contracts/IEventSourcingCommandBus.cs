using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Net.Abstractions.Identities;

namespace EventSourcing.Net.Abstractions.Contracts;

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
    Task<ICommandExecutionResult<TId>> Send<TId, TPayload>(TenantId tenantId, PrincipalId principalId, string source, 
        TId aggregateId, TPayload commandPayload, 
        CancellationToken cancellationToken = default)
        where TPayload : ICommand;
        
    /// <summary>
    /// Get publication awaiter to await publication result.
    /// </summary>
    IPublicationAwaiter PublicationAwaiter { get; }
}