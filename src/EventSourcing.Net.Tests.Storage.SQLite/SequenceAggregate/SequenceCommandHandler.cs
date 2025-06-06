namespace EventSourcing.Net.Tests.Storage.SQLite.SequenceAggregate;

using Engine;
using EventSourcing.Net.Abstractions.Contracts;

public class SequenceCommandHandler : 
    CommandHandler<int, SequenceAggregate>,
    ICommandHandler<int, AddItemCommand>
{
    public SequenceCommandHandler() : base(id => new SequenceAggregate(id))
    {
    }
    
    public Task<ICommandExecutionResult<int>> Handle(ICommandEnvelope<int, AddItemCommand> cmd)
    {
        return Update(cmd, x => x.AddItem(cmd));
    }
}