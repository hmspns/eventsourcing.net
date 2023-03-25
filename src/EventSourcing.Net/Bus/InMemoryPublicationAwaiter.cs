using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Net;

public sealed class InMemoryPublicationAwaiter : IPublicationAwaiter
{
    public void MarkAsReady(CommandSequenceId sequenceId)
    {
        throw new NotSupportedException();
    }

    public Task WaitForPublication(CommandSequenceId sequenceId, TimeSpan timeout = default)
    {
        throw new NotSupportedException();
    }
}