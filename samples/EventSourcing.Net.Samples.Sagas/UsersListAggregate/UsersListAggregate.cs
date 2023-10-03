namespace EventSourcing.Net.Samples.Sagas.UsersListAggregate;

using Abstractions.Contracts;
using Engine;
using Engine.Extensions;

public sealed class UsersListAggregate : Aggregate<string, UsersListState, UsersListStateMutator>
{
    public UsersListAggregate(string aggregateId) : base(aggregateId, new UsersListStateMutator())
    {
    }

    public ICommandExecutionResult<string> AddUser(ICommandEnvelope<string, AddUserToListCommand> cmd)
    {
        if (!State.Users.ContainsKey(cmd.Payload.UserId))
        {
            Apply(cmd, new UserAddedToListEvent(cmd.Payload.UserId, cmd.Payload.Name));
        }

        return this.OkIfChanges(cmd);
    }
}