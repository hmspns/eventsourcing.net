using EventSourcing.Abstractions.Contracts;

namespace EventSourcing.Samples.Persistence.AccountAggregate;

public record AccountCreatedEvent(string OwnerName) : IEvent;

public record AccountReplenishedEvent(Guid OperationId, decimal Amount) : IEvent;

public record AccountWithdrawnEvent(Guid OperationId, decimal Amount) : IEvent;

public record AccountClosedEvent : IEvent;