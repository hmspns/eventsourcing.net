namespace EventSourcing.Net.Samples.Sagas.UserAggregate;

using Abstractions.Contracts;
using Abstractions.ServiceRegistration;

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