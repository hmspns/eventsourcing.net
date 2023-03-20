using System.Threading.Tasks;
using EventSourcing.Abstractions;

namespace EventSourcing.Core.Contracts
{
    /// <summary>
    /// Command bus.
    /// </summary>
    public interface ICommandBus
    {
        /// <summary>
        /// Send the command.
        /// </summary>
        /// <param name="id">Aggregate id.</param>
        /// <param name="command">Command.</param>
        /// <returns>Result of executed command.</returns>
        Task<ICommandExecutionResult<TId>> Send<TId>(TId id, ICommand command);
    }

    /// <summary>
    /// Event bus.
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Send the event to the bus.
        /// </summary>
        /// <param name="eventEnvelope">Event envelope.</param>
        Task Send(IEventEnvelope eventEnvelope);
    }
}