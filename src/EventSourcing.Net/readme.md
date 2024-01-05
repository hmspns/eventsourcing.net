## EventSourcing.Net

EventSourcing.Net is a framework inspired by the principles of [CQRS](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs) and [event sourcing](https://docs.microsoft.com/en-us/azure/architecture/patterns/event-sourcing) architecture.

### Key features
- Built-in snapshots
- Optimized for high performance with low memory consumption
- Based on SOLID principles and extensibility
- Built-in support for JSON and protobuf serialization
- Full support for MediatR as the command bus

### Getting started

Install package from [nuget.org](https://www.nuget.org/packages/EventSourcing.Net)

```
Install-Package EventSourcing.Net
```

or

```
dotnet add package EventSourcing.Net
```

Register in the DI container

```csharp  
Assembly assembly = Assembly.GetExecutingAssembly();  
  
IServiceCollection services = new ServiceCollection();  
services.AddEventSourcing(options =>  
{       
     options.Bus.RegisterCommandHandlers(assembly);
     options.Bus.RegisterEventConsumers(assembly);
});

// get instance of the service provider
IServiceProvider provider = services.BuildServiceProvider();  
  
// start event sourcing engine  
await provider.StartEventSourcingEngine();
```

### Commands

Command can be a simple class/record and should implement interface `ICommand`:

```csharp
public record CreateUserCommand(string Name, DateTime BirthDate, string PhoneNumber) : ICommand;
```

### Events

Event is a simple class/record that should implement interface `IEvent`:
```csharp
public record UserCreatedEvent(string Name, DateTime BirthDate, string PhoneNumber) : IEvent;
```

### Aggregate

Aggregate is the place where we generating events based on incoming commads. It should inherit from `Aggregate<TId, TState, TStateMutator>`:

```csharp
public class UserAggregate : Aggregate<Guid, UserState, UserStateMutator>  
{  
    public UserAggregate(Guid id) : base(id, new UserStateMutator())  
    {  
    }  
  
    public CommandExecutionResult<Guid> CreateUser(ICommandEnvelope<Guid, CreateUserCommand> cmd)  
    {  
        if (!State.IsCreated)  
        {  
            Apply(cmd, new UserCreatedEvent(cmd.Payload.Name, cmd.Payload.BirthDate, cmd.Payload.PhoneNumber));  
        }  
          
        return CommandExecutionResult<Guid>.OkIfChanges(this, cmd);  
    }
}
```

In the example above aggregate using type `Guid` as the type of id, `UserState` as type of state and `UserStateMutator` as type of state mutator.

In EventSourcing.Net aggregates responds only for processing events and nothing more.
Event stream will be loaded by framework and passed to instance of aggregate. At the moment of excecutin `CreateUser` method state will be exists and available.
After each call of `Apply` state will be changed by mutator with respect to passed event.

### State

State is the simple POCO object, that can be serialized to snapshot. It might looks like:

```csharp
public record UserState  
{  
    /// <summary>  
    /// Property to indicate that current user is exists. Exists means UserCreatedEvent was handled.
    /// </summary>
    public bool IsCreated { get; set; }  
      
    public string Name { get; set; }  
      
    public DateTime BirthDate { get; set; }  
      
    public string PhoneNumber { get; set; }  
}
```

### State mutator

State mutator is the place where incoming events update the state. It should be inherited from `StateMutator<TState>`

```csharp
public class UserStateMutator : StateMutator<UserState>  
{  
    /// <summary>  
    /// Default state
    /// </summary>
    public override UserState DefaultState => new UserState()  
    {  
        IsCreated = false
    };  
  
    public UserStateMutator()  
    {  
        // here we register our handlers witch will update the state     
        Register<Guid, UserCreatedEvent>(Handle); 
    }  
      
    private UserState Handle(IEventEnvelope<Guid, UserCreatedEvent> e, UserState state)  
    {  
        state.Name = e.Payload.Name;  
        state.BirthDate = e.Payload.BirthDate;  
        state.PhoneNumber = e.Payload.PhoneNumber;  
        state.IsCreated = true; // mark that current user is created now  
  
        return state;  
    }
}
```

### Command handler

Command handler is the place where the flow starting.
```csharp
public class UserCommandHandler : CommandHandler<Guid, UserAggregate>  
{  
    public UserCommandHandler() : base(aggregateId => new UserAggregate(aggregateId))  
    {  
    }  
  
    public async Task<ICommandExecutionResult<Guid>> CreateUser(ICommandEnvelope<Guid, CreateUserCommand> cmd, CancellationToken token)  
    {  
        return await Update(cmd, aggregate => aggregate.CreateUser(cmd), token);  
    }
}
```

Built in bus will call command handlers by convention:
- Command handler should be inherited from `CommandHadler<TId, TAggregate>`
- It should contains methods that accept `ICommandEnvelope<TId, TCommand>` and optional `CancellationToken`.
- It should returns `Task<ICommandExecutionResult<TId>>`

### Event consumers

The final part of the flow is the consumers of the events.

Every consumer should implement one or more interfaces `IEventConsumer<TId, TEvent>`:

```csharp
public class UserProjector : IEventConsumer<Guid, UserCreatedEvent>
{  
    public Task Consume(IEventEnvelope<Guid, UserCreatedEvent> envelope)  
    {  
        Console.WriteLine(envelope.Payload);  
        return Task.CompletedTask;  
    }
}
```

### Sending commands

Get the instance of `IEventSourcingCommandBus` from DI container and use it:

```csharp
public async Task CreateUser(IEventSourcingCommandBus bus)  
{  
    CreateUserCommand cmd = new CreateUserCommand("Test", new DateTime(2000, 1, 1), "123-456-789");  
    ICommandExecutionResult<Guid> result = await bus.Send(Guid.NewGuid(), cmd);  
}
```

### Extensions

- [EventSourcing.Net.Storage.Postgres](https://www.nuget.org/packages/EventSourcing.Net.Storage.Postgres/) - the implementation of an event store for Postgres.
- [EventSourcing.Net.Storage.Redis](https://www.nuget.org/packages/EventSourcing.Net.Storage.Redis/) - the implementation of an snapshot store for Redis.
- [EventSourcing.Net.Bus.Mediatr](https://www.nuget.org/packages/EventSourcing.Net.Bus.Mediatr/) - the implementation of EventSourcing.Net bus based on MediatR.
- [EventSourcing.Net.Serialization.NewtonsoftJson](https://www.nuget.org/packages/EventSourcing.Net.Serialization.NewtonsoftJson/) - the implementation of EventSourcing.Net serialization based on Newtonsoft.Json.
- [EventSourcing.Net.Serialization.ProtobufNet](https://www.nuget.org/packages/EventSourcing.Net.Serialization.ProtobufNet/) - the implementation of EventSourcing.Net serialization based on protobuf-net.
