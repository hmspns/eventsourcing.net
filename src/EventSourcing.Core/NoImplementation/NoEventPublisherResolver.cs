using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Core.NoImplementation;

public class NoEventPublisherResolver : IResolveEventPublisher
{
    private static IEventPublisher _publisher = new NoEventPublisher();
    
    public IEventPublisher Get(TenantId tenantId)
    {
        return _publisher;
    }
}

public class NoEventPublisher : IEventPublisher
{
    public Task Publish(ICommandEnvelope commandEnvelope, IReadOnlyList<IEventEnvelope> events)
    {
        return Task.CompletedTask;
    }
}