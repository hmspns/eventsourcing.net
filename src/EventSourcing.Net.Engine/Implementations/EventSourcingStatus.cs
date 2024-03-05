namespace EventSourcing.Net.Engine.Implementations;

using Abstractions;

/// <inheritdoc />
public sealed class EventSourcingStatus : IEventSourcingStatus
{
    public bool IsStarted { get; internal set; }
}