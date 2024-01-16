// See https://aka.ms/new-console-template for more information

using System.Reflection;
using EventSourcing.Net;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Engine.Implementations;
using EventSourcing.Net.Samples.Sagas;
using EventSourcing.Net.Samples.Sagas.UserAggregate;
using EventSourcing.Net.Samples.Sagas.UsersListAggregate;
using Microsoft.Extensions.DependencyInjection;

Assembly assembly = Assembly.GetExecutingAssembly();

IServiceCollection services = new ServiceCollection();
services.AddEventSourcing(options =>
{
    options.Bus.RegisterCommandHandlers(assembly);
    options.Bus.RegisterEventConsumers(assembly);
    options.Bus.RegisterSagaConsumers(assembly);
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

AggregateStateLoader<string, UsersListAggregate, UsersListState> usersListLoader = new AggregateStateLoader<string, UsersListAggregate, UsersListState>();
UsersListState usersListState = await usersListLoader.GetState(Constants.DEFAULT_USERS_LIST);

Console.WriteLine("Users list state: " + usersListState);