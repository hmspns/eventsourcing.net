namespace EventSourcing.Net.Samples.Sagas.UserAggregate;

using Abstractions.Contracts;

public record CreateUserCommand(string Name, DateTime BirthDate, string PhoneNumber) : ICommand;

public record UpdateUserCommand(string Name, string PhoneNumber) : ICommand;

public record DeleteUserCommand() : ICommand;