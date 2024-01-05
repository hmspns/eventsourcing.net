namespace EventSourcing.Net.Samples.CustomIdTypes.GuidAggregate;

using Abstractions.Contracts;
using Engine;
using Engine.Extensions;

public sealed class UserStateMutator : StateMutator<UserState>
{
    public override UserState DefaultState => new UserState();

    public UserStateMutator()
    {
        Register<GuidId, UserAddedEvent>(Handle);
    }

    private UserState Handle(IEventEnvelope<GuidId, UserAddedEvent> e, UserState state)
    {
        state.Users.Add(e.Payload.Name);

        return state;
    }
}

public sealed class GuidUserAggregate : Aggregate<GuidId, UserState, UserStateMutator>
{
    public GuidUserAggregate(GuidId aggregateId) : base(aggregateId, new UserStateMutator())
    {
    }

    public ICommandExecutionResult<GuidId> AddUser(ICommandEnvelope<GuidId, AddUserCommand> cmd)
    {
        Apply(cmd, new UserAddedEvent(cmd.Payload.Name));

        return this.OkIfChanges(cmd);
    }
}

public sealed class UserCommandHandler : CommandHandler<GuidId, GuidUserAggregate>
{
    public UserCommandHandler() : base(aggregateId => new GuidUserAggregate(aggregateId))
    {
    }

    public Task<ICommandExecutionResult<GuidId>> AddUser(ICommandEnvelope<GuidId, AddUserCommand> cmd)
    {
        return Update(cmd, aggregate => aggregate.AddUser(cmd));
    }
}