﻿namespace EventSourcing.Net.Samples.Sagas.UserAggregate;

using Abstractions.Contracts;
using Engine;

public class UserCommandHandler : CommandHandler<Guid, UserAggregate>
{
    public UserCommandHandler() :
        base(aggregateId => new UserAggregate(aggregateId))
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