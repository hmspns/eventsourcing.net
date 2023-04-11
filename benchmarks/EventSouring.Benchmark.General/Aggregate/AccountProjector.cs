using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.ServiceRegistration;

namespace EventSouring.Benchmark.General.Aggregate;

public sealed class AccountProjector :
    IEventConsumer<Guid, AccountCreatedEvent>,
    IEventConsumer<Guid, AccountReplenishedEvent>,
    IEventConsumer<Guid, AccountWithdrawnEvent>,
    IEventConsumer<Guid, AccountClosedEvent>
{
    public AccountProjector()
    {
    }

    public Task Consume(IEventEnvelope<Guid, AccountCreatedEvent> envelope)
    {
        return Task.CompletedTask;
    }

    public Task Consume(IEventEnvelope<Guid, AccountReplenishedEvent> envelope)
    {
        return Task.CompletedTask;
    }

    public Task Consume(IEventEnvelope<Guid, AccountWithdrawnEvent> envelope)
    {
        return Task.CompletedTask;
    }

    public Task Consume(IEventEnvelope<Guid, AccountClosedEvent> envelope)
    {
        return Task.CompletedTask;
    }
}