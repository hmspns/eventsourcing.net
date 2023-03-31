﻿using EventSourcing.Abstractions.Contracts;
using EventSourcing.Core;

namespace EventSourcing.Samples.Simple.UserAggregate;

public class UserCommandHandler : CommandHandler<Guid, UserAggregate>
{
    public UserCommandHandler(IEventSourcingEngine engine) :
        base(aggregateId => new UserAggregate(aggregateId), engine)
    {
    }

    public async Task<ICommandExecutionResult<Guid>> CreateUser(ICommandEnvelope<Guid, CreateUserCommand> cmd, CancellationToken token)
    {
        return await Update(cmd, aggregate => aggregate.CreateUser(cmd), token);
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