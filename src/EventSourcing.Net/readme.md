## EventSourcing.Net

This is .net framework which implement EventSourcing pattern in a simple and effective way.

### Get Started

Register eventsourcing net in the DI container

```csharp
Assembly assembly = Assembly.GetExecutingAssembly();

IServiceCollection services = new ServiceCollection();
services.AddEventSourcing(options =>
    {
        options.Bus.RegisterCommandHandlers(assembly);
        options.Bus.RegisterEventConsumers(assembly);
    });
IServiceProvider provider = services.BuildServiceProvider();

// start event sourcing engine
await provider.StartEventSourcingEngine();
```

Send a command to produce events

```csharp
public record CreateUserCommand(string Name, DateTime BirthDate, string PhoneNumber) : ICommand;
```

```csharp
CreateUserCommand cmd = new CreateUserCommand("John Doe", DateTime.UtcNow, "123-456-789");
IEventSourcingCommandBus bus = provider.GetRequiredService<IEventSourcingCommandBus>();

ICommandExecutionResult<Guid> result = await bus.Send(Guid.NewGuid(), cmd);
```

Full example available on the [project page](https://github.com/hmspns/eventsourcing.net).
