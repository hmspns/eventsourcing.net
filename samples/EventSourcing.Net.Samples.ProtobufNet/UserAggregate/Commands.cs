using EventSourcing.Net.Abstractions.Contracts;
using ProtoBuf;

namespace EventSourcing.Net.Samples.ProtobufNet.UserAggregate;

[ProtoContract]
public record CreateUserCommand(
    [property: ProtoMember(1)] string Name,
    [property: ProtoMember(2)] DateTime BirthDate,
    [property: ProtoMember(3)] string PhoneNumber
) : ICommand;

[ProtoContract]
public record UpdateUserCommand(
    [property: ProtoMember(1)] string Name,
    [property: ProtoMember(2)] string PhoneNumber
) : ICommand;

[ProtoContract]
public record DeleteUserCommand() : ICommand;