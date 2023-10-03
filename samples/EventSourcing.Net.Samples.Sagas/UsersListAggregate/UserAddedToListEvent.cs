namespace EventSourcing.Net.Samples.Sagas.UsersListAggregate;

using Abstractions.Contracts;

public record UserAddedToListEvent(Guid UserId, string Name) : IEvent;