namespace EventSourcing.Net.Tests.SequenceAggregate;

using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Engine;

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