namespace EventSourcing.Net.Tests.Storage.SQLite.SequenceAggregate;

using Engine;
using Engine.Extensions;
using EventSourcing.Net.Abstractions.Contracts;

public class SequenceAggregate : Aggregate<int, SequenceState, SequenceStateMutator>
{
    public SequenceAggregate(int aggregateId) : base(aggregateId, new SequenceStateMutator())
    {
    }

    public ICommandExecutionResult<int> AddItem(ICommandEnvelope<int, AddItemCommand> cmd)
    {
        Apply(cmd, new ItemAddedEvent(State.CurrentPosition + 1));
        return this.OkIfChanges(cmd);
    }
}