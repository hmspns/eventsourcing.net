using EventSourcing.Abstractions;
using EventSourcing.Core;
using EventSourcing.Core.Contracts;

namespace EventSourcing.Samples.Simple.UserAggregate;

public class UserCommandHandler : CommandHandler<Guid, UserAggregate>
{
    public UserCommandHandler(IEventSourcingEngine engine) :
        base(aggregateId => new UserAggregate(aggregateId), engine)
    {
    }

    public async Task<ICommandExecutionResult<Guid>> CreateUser(ICommandEnvelope<Guid, CreateUserCommand> cmd)
    {
        return await Update(cmd, aggregate => aggregate.CreateUser(cmd));
    }

    public async Task<ICommandExecutionResult<Guid>> UpdateUser(ICommandEnvelope<Guid, UpdateUserCommand> cmd)
    {
        return await Update(cmd, aggregate => aggregate.UpdateUser(cmd));
    }

    public async Task<ICommandExecutionResult<Guid>> DeleteUser(ICommandEnvelope<Guid, DeleteUserCommand> cmd)
    {
        return await Update(cmd, aggregate => aggregate.DeleteUser(cmd));
    }
}