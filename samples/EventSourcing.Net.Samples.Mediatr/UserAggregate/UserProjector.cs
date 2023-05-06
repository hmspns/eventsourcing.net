using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.ServiceRegistration;
using EventSourcing.Net.Bus.Mediatr;
using MediatR;

namespace EventSourcing.Net.Samples.Mediatr.UserAggregate;

public class UserProjector :
    INotificationHandler<MediatrEventEnvelope<Guid, UserCreatedEvent>>,
    INotificationHandler<MediatrEventEnvelope<Guid, UserNameChangedEvent>>,
    INotificationHandler<MediatrEventEnvelope<Guid, UserPhoneChangedEvent>>,
    INotificationHandler<MediatrEventEnvelope<Guid, UserDeletedEvent>>
{
    public Task Handle(MediatrEventEnvelope<Guid, UserCreatedEvent> envelope, CancellationToken cancellationToken)
    {
        Console.WriteLine(envelope.Payload);
        return Task.CompletedTask;
    }
    
    public Task Handle(MediatrEventEnvelope<Guid, UserNameChangedEvent> envelope, CancellationToken cancellationToken)
    {
        Console.WriteLine(envelope.Payload);
        return Task.CompletedTask;
    }
    
    public Task Handle(MediatrEventEnvelope<Guid, UserPhoneChangedEvent> envelope, CancellationToken cancellationToken)
    {
        Console.WriteLine(envelope.Payload);
        return Task.CompletedTask;
    }
    
    public Task Handle(MediatrEventEnvelope<Guid, UserDeletedEvent> envelope, CancellationToken cancellationToken)
    {
        Console.WriteLine(envelope.Payload);
        return Task.CompletedTask;
    }
}