using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Net;

public sealed class InMemoryPublicationAwaiter : IPublicationAwaiter
{
    public void MarkAsReady(CommandSequenceId sequenceId)
    {
        // do nothing here
    }

    public Task WaitForPublication(CommandSequenceId sequenceId, TimeSpan timeout = default)
    {
        return Task.CompletedTask;
    }
}