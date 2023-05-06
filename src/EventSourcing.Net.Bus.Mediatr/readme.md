This package provides functionality to use Mediatr as the bus for EventSourcing.Net.

### Configuration example

```csharp
public void RegisterEventSourcing(IServiceCollection services, IConfiguration configuration)  
{  
    services.AddEventSourcing(options =>  
    {  
        options.Bus.UseMediatr();
    }); 
}
```

this code will register:
- MediatrEventSourcingCommandBus as transient.
- MediatrEventPublisherResolver as transient.

### Command handlers

Commands will be sent, so command handlers will looks like:

```csharp
public class UserCommandHandler : CommandHandler<Guid, UserAggregate>, 
    IRequestHandler<MediatrCommandEnvelope<Guid, CreateUserCommand>, ICommandExecutionResult<Guid>>
{
    public UserCommandHandler() : base(aggregateId => new UserAggregate(aggregateId))
    {
    }
    
    public async Task<ICommandExecutionResult<Guid>> Handle(MediatrCommandEnvelope<Guid, CreateUserCommand> cmd, CancellationToken token)
    {
        return await Update(cmd, aggregate => aggregate.CreateUser(cmd), token);
    }
}
```

### Events consumer

Events will be published, so event handlers will looks like:

```csharp
public class UserProjector :
    INotificationHandler<MediatrEventEnvelope<Guid, UserCreatedEvent>>
{
    public Task Handle(MediatrEventEnvelope<Guid, UserCreatedEvent> envelope, CancellationToken cancellationToken)
    {
        Console.WriteLine(envelope.Payload);
        return Task.CompletedTask;
    }
}
```

Check [EventSourcing.Net](https://github.com/hmspns/eventsourcing.net) to find full documentation.