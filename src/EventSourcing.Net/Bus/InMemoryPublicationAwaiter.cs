using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Abstractions.Contracts;

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