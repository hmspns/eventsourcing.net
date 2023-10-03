namespace EventSourcing.Net.Samples.Sagas.UsersListAggregate;

using Abstractions.Contracts;
using Engine;

public sealed class UsersListCommandHandler : CommandHandler<string, UsersListAggregate>
{
    public UsersListCommandHandler() : base(id => new UsersListAggregate(id))
    {
    }

    public Task<ICommandExecutionResult<string>> AddUser(ICommandEnvelope<string, AddUserToListCommand> cmd)
    {
        return Update(cmd, aggregate => aggregate.AddUser(cmd));
    }
}