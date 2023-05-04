using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Benchmark.Shared.Account;

public record AccountCreatedEvent(string OwnerName) : IEvent;

public record AccountReplenishedEvent(Guid OperationId, decimal Amount) : IEvent;

public record AccountWithdrawnEvent(Guid OperationId, decimal Amount) : IEvent;

public record AccountClosedEvent : IEvent;