namespace EventSourcing.Net.Tests.Storage.Postgres.SequenceAggregate;

using Abstractions.Contracts;

public record SequenceState
{
    public List<int> Items { get; set; }
    
    public int CurrentPosition { get; set; }
}

public record AddItemCommand() : ICommand;

public record ItemAddedEvent(int Position) : IEvent;