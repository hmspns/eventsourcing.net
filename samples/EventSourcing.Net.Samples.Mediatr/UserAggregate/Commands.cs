using EventSourcing.Net.Abstractions.Contracts;
using MediatR;

namespace EventSourcing.Net.Samples.Mediatr.UserAggregate;

public record CreateUserCommand(string Name, DateTime BirthDate, string PhoneNumber) : ICommand, IRequest<CreateUserCommand>;

public record UpdateUserCommand(string Name, string PhoneNumber) : ICommand, IRequest<UpdateUserCommand>;

public record DeleteUserCommand() : ICommand, IRequest<DeleteUserCommand>;