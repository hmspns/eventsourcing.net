using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Benchmark.Shared.Account;

public record CreateAccountCommand(string OwnerName) : ICommand;

public record ReplenishAccountCommand(Guid OperationId, decimal Amount) : ICommand;

public record WithdrawAccountCommand(Guid OperationId, decimal Amount) : ICommand;

public record CloseAccountCommand() : ICommand;