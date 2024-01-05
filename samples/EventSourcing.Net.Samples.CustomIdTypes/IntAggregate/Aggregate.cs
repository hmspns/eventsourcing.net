namespace EventSourcing.Net.Samples.CustomIdTypes.IntAggregate;

using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Engine;
using EventSourcing.Net.Engine.Extensions;

public sealed class UserStateMutator : StateMutator<UserState>
{
    public override UserState DefaultState => new UserState();

    public UserStateMutator()
    {
        Register<IntId, UserAddedEvent>(Handle);
    }

    private UserState Handle(IEventEnvelope<IntId, UserAddedEvent> e, UserState state)
    {
        state.Users.Add(e.Payload.Name);

        return state;
    }
}

public sealed class IntUserAggregate : Aggregate<IntId, UserState, UserStateMutator>
{
    public IntUserAggregate(IntId aggregateId) : base(aggregateId, new UserStateMutator())
    {
    }

    public ICommandExecutionResult<IntId> AddUser(ICommandEnvelope<IntId, AddUserCommand> cmd)
    {
        Apply(cmd, new UserAddedEvent(cmd.Payload.Name));

        return this.OkIfChanges(cmd);
    }
}

public sealed class UserCommandHandler : CommandHandler<IntId, IntUserAggregate>
{
    public UserCommandHandler() : base(aggregateId => new IntUserAggregate(aggregateId))
    {
    }

    public Task<ICommandExecutionResult<IntId>> AddUser(ICommandEnvelope<IntId, AddUserCommand> cmd)
    {
        return Update(cmd, aggregate => aggregate.AddUser(cmd));
    }
}