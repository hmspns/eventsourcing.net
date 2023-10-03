namespace EventSourcing.Net.Samples.Sagas.UsersListAggregate;

using Abstractions.Contracts;

public record AddUserToListCommand(Guid UserId, string Name) : ICommand;