namespace EventSourcing.Net.Abstractions.Contracts;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Optional interface to mark class as the command handler.
/// </summary>
/// <typeparam name="TId">Type of the aggregate id.</typeparam>
/// <typeparam name="TPayload">Type of the command payload.</typeparam>
/// <remarks>It's not necessary to use this interface. Command handlers resolving by convention:
/// <ul>
/// <li>Command handler should be inherited from <i>CommandHandler&lt;TId, TAggregate&gt;</i></li>
/// <li>It should contains public methods that accept <i>ICommandEnvelope&lt;TId, TCommand&gt;</i> and optional `CancellationToken`;</li>
/// <li>Methods should returns <i>Task&gt;ICommandExecutionResult&gt;TId&gt;&gt;</i>;</li>
/// </ul>
/// However it might helps your IDE create method for you.
/// </remarks>
public interface ICommandHandler<TId, in TPayload>
    where TPayload : ICommand
{
    /// <summary>
    /// Handle the command.
    /// </summary>
    /// <param name="cmd">Command.</param>
    /// <returns>Status of the command execution.</returns>
    Task<ICommandExecutionResult<TId>> Handle(ICommandEnvelope<TId, TPayload> cmd);
}

/// <summary>
/// Optional interface to mark class as the command handler with cancellation support.
/// </summary>
/// <typeparam name="TId">Type of the aggregate id.</typeparam>
/// <typeparam name="TPayload">Type of the command payload.</typeparam>
/// <remarks>It's not necessary to use this interface. Command handlers resolving by convention:
/// <ul>
/// <li>Command handler should be inherited from <i>CommandHandler&lt;TId, TAggregate&gt;</i></li>
/// <li>It should contains public methods that accept <i>ICommandEnvelope&lt;TId, TCommand&gt;</i> and optional `CancellationToken`;</li>
/// <li>Methods should returns <i>Task&gt;ICommandExecutionResult&gt;TId&gt;&gt;</i>;</li>
/// </ul>
/// However it might helps your IDE create method for you.
/// </remarks>
public interface ICancellableCommandHandler<TId, in TPayload>
    where TPayload : ICommand
{
    /// <summary>
    /// Handle the command.
    /// </summary>
    /// <param name="cmd">Command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Status of the command execution.</returns>
    Task<ICommandExecutionResult<TId>> Handle(ICommandEnvelope<TId, TPayload> cmd, CancellationToken cancellationToken);
}