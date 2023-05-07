
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Bus.Mediatr;
using EventSourcing.Net.Engine;
using MediatR;

namespace EventSourcing.Net.Samples.Mediatr.UserAggregate;

public class UserCommandHandler : CommandHandler<Guid, UserAggregate>, 
    IRequestHandler<MediatrCommandEnvelope<Guid, CreateUserCommand>, ICommandExecutionResult<Guid>>,
    IRequestHandler<MediatrCommandEnvelope<Guid, UpdateUserCommand>, ICommandExecutionResult<Guid>>,
    IRequestHandler<MediatrCommandEnvelope<Guid, DeleteUserCommand>, ICommandExecutionResult<Guid>>
{
    public UserCommandHandler() :
        base(aggregateId => new UserAggregate(aggregateId))
    {
    }
    
    public async Task<ICommandExecutionResult<Guid>> Handle(MediatrCommandEnvelope<Guid, CreateUserCommand> cmd, CancellationToken token)
    {
        return await Update(cmd, aggregate => aggregate.CreateUser(cmd), token);
    }

    public async Task<ICommandExecutionResult<Guid>> Handle(MediatrCommandEnvelope<Guid, UpdateUserCommand> cmd, CancellationToken token)
    {
        return await Update(cmd, aggregate => aggregate.UpdateUser(cmd), token);
    }

    public async Task<ICommandExecutionResult<Guid>> Handle(MediatrCommandEnvelope<Guid, DeleteUserCommand> cmd, CancellationToken token)
    {
        return await Update(cmd, aggregate => aggregate.DeleteUser(cmd), token);
    }
}