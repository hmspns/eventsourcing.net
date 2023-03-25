using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.ServiceRegistration;

namespace EventSourcing.Samples.Simple.UserAggregate;

public class UserProjector :
    IEventConsumer<Guid, UserCreatedEvent>,
    IEventConsumer<Guid, UserNameChangedEvent>,
    IEventConsumer<Guid, UserPhoneChangedEvent>,
    IEventConsumer<Guid, UserDeletedEvent>
{
    public Task Consume(IEventEnvelope<Guid, UserCreatedEvent> envelope)
    {
        Console.WriteLine(envelope.Payload);
        return Task.CompletedTask;
    }

    public Task Consume(IEventEnvelope<Guid, UserNameChangedEvent> envelope)
    {
        Console.WriteLine(envelope.Payload);
        return Task.CompletedTask;
    }

    public Task Consume(IEventEnvelope<Guid, UserPhoneChangedEvent> envelope)
    {
        Console.WriteLine(envelope.Payload);
        return Task.CompletedTask;
    }

    public Task Consume(IEventEnvelope<Guid, UserDeletedEvent> envelope)
    {
        Console.WriteLine(envelope.Payload);
        return Task.CompletedTask;
    }
}