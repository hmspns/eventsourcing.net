namespace EventSourcing.Net.Samples.Sagas.UsersListAggregate;

using Abstractions.Contracts;
using Engine;

public sealed class UsersListStateMutator : StateMutator<UsersListState>
{
    public override UsersListState DefaultState => new UsersListState();

    public UsersListStateMutator()
    {
        Register<string, UserAddedToListEvent>(Handle);
    }

    private UsersListState Handle(IEventEnvelope<string, UserAddedToListEvent> e, UsersListState state)
    {
        state.Users[e.Payload.UserId] = e.Payload.Name;

        return state;
    }
}