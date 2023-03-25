// using EventSourcing.Abstractions.Contracts;
//
// public delegate Task<ICommandExecutionResult<TId>> WithoutCancellationCommandDelegate<TId, in TCommand>
//     (ICommandEnvelope<TId, TCommand> command) where TCommand : ICommand;
//
// public delegate Task<ICommandExecutionResult<TId>> WithCancellationCommandDelegate<TId, in TCommand>
//     (ICommandEnvelope<TId, TCommand> command, CancellationToken cancellationToken) where TCommand : ICommand;