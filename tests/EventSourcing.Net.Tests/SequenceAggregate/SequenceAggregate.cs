namespace EventSourcing.Net.Tests.SequenceAggregate;

using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Engine;
using EventSourcing.Net.Engine.Extensions;

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