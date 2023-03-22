using EventSourcing.Abstractions;
using EventSourcing.Abstractions.Contracts;

namespace EventSourcing.Samples.Simple.UserAggregate;

public record UserCreatedEvent(string Name, DateTime BirthDate, string PhoneNumber) : IEvent;

public record UserNameChangedEvent(string OldName, string NewName) : IEvent;

public record UserPhoneChangedEvent(string OldPhoneNumber, string NewPhoneNumber) : IEvent;

public record UserDeletedEvent() : IEvent;