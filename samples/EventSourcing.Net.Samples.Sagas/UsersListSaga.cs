namespace EventSourcing.Net.Samples.Sagas;

using Abstractions.Contracts;
using Abstractions.ServiceRegistration;
using UserAggregate;
using UsersListAggregate;

public sealed class UsersListSaga : IEventConsumer<Guid, UserCreatedEvent>
{
    private readonly ISagaEventSourcingCommandBus _bus;

    public UsersListSaga(ISagaEventSourcingCommandBus bus)
    {
        _bus = bus;
    }
    
    public async Task Consume(IEventEnvelope<Guid, UserCreatedEvent> envelope)
    {
        AddUserToListCommand cmd = new AddUserToListCommand(envelope.AggregateId, envelope.Payload.Name);
        await _bus.Send(Constants.DEFAULT_USERS_LIST, envelope, cmd);
    }
}