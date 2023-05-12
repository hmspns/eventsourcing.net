using System.Reflection;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net;
using EventSourcing.Net.Engine.Implementations;
using EventSourcing.Net.Samples.Simple.UserAggregate;
using Microsoft.Extensions.DependencyInjection;

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

CreateUserCommand cmd = new CreateUserCommand("Test", DateTime.UtcNow, "123-456-789");
IEventSourcingCommandBus bus = provider.GetRequiredService<IEventSourcingCommandBus>();

Guid aggregateId = Guid.NewGuid();
ICommandExecutionResult<Guid> result = await bus.Send(aggregateId, cmd);

Console.WriteLine("CommandExecutionResult: " + result);

AggregateStateLoader<Guid, UserAggregate, UserState> loader = new AggregateStateLoader<Guid, UserAggregate, UserState>();
UserState state = await loader.GetState(aggregateId);

Console.WriteLine("State: " + state);