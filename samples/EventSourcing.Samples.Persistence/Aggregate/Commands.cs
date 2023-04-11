using EventSourcing.Abstractions.Contracts;

namespace EventSourcing.Samples.Persistence.Aggregate;

public record CreateAccountCommand(string OwnerName) : ICommand;

public record ReplenishAccountCommand(Guid OperationId, decimal Amount) : ICommand;

public record WithdrawAccountCommand(Guid OperationId, decimal Amount) : ICommand;

public record CloseAccountCommand(decimal Amount) : ICommand;