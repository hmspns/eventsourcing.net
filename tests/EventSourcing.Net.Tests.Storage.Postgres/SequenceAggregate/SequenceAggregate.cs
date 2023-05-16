namespace EventSourcing.Net.Tests.Storage.Postgres.SequenceAggregate;

using Abstractions.Contracts;
using Engine;
using Engine.Extensions;

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