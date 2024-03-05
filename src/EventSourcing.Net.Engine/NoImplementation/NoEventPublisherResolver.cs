using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;

namespace EventSourcing.Net.Engine.NoImplementation;

public sealed class NoEventPublisherResolver : IResolveEventPublisher
{
    private static readonly IEventPublisher _publisher = new NoEventPublisher();
    
    public IEventPublisher Get(TenantId tenantId)
    {
        return _publisher;
    }
}

public sealed class NoEventPublisher : IEventPublisher
{
    public Task Publish(ICommandEnvelope? commandEnvelope, IReadOnlyList<IEventEnvelope> events)
    {
        return Task.CompletedTask;
    }
}