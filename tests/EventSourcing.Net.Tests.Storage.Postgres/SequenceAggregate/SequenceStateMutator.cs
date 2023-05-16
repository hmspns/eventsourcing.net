namespace EventSourcing.Net.Tests.Storage.Postgres.SequenceAggregate;

using Abstractions.Contracts;
using Engine;

public sealed class SequenceStateMutator : StateMutator<SequenceState>
{
    public override SequenceState DefaultState => new SequenceState()
    {
        Items = new List<int>()
    };

    public SequenceStateMutator()
    {
        Register<int, ItemAddedEvent>(Handle);
    }

    private SequenceState Handle(IEventEnvelope<int, ItemAddedEvent> e, SequenceState state)
    {
        state.Items.Add(e.Payload.Position);
        state.CurrentPosition = e.Payload.Position;

        return state;
    }
}