using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Net.Samples.NewtonsoftJson.UserAggregate;

public record CreateUserCommand(string Name, DateTime BirthDate, string PhoneNumber) : ICommand;

public record UpdateUserCommand(string Name, string PhoneNumber) : ICommand;

public record DeleteUserCommand() : ICommand;