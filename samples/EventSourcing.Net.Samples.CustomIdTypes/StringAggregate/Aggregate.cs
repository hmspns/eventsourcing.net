namespace EventSourcing.Net.Samples.CustomIdTypes.StringAggregate;

using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Engine;
using EventSourcing.Net.Engine.Extensions;

public sealed class UserStateMutator : StateMutator<UserState>
{
    public override UserState DefaultState => new UserState();

    public UserStateMutator()
    {
        Register<StringId, UserAddedEvent>(Handle);
    }

    private UserState Handle(IEventEnvelope<StringId, UserAddedEvent> e, UserState state)
    {
        state.Users.Add(e.Payload.Name);

        return state;
    }
}

public sealed class StringUserAggregate : Aggregate<StringId, UserState, UserStateMutator>
{
    public StringUserAggregate(StringId aggregateId) : base(aggregateId, new UserStateMutator())
    {
    }

    public ICommandExecutionResult<StringId> AddUser(ICommandEnvelope<StringId, AddUserCommand> cmd)
    {
        Apply(cmd, new UserAddedEvent(cmd.Payload.Name));

        return this.OkIfChanges(cmd);
    }
}

public sealed class UserCommandHandler : CommandHandler<StringId, StringUserAggregate>
{
    public UserCommandHandler() : base(aggregateId => new StringUserAggregate(aggregateId))
    {
    }

    public Task<ICommandExecutionResult<StringId>> AddUser(ICommandEnvelope<StringId, AddUserCommand> cmd)
    {
        return Update(cmd, aggregate => aggregate.AddUser(cmd));
    }
}