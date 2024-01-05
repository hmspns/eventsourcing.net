

using System.Reflection;
using EventSourcing.Net;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Engine.Implementations;
using EventSourcing.Net.Samples.CustomIdTypes;
using EventSourcing.Net.Samples.CustomIdTypes.GuidAggregate;
using EventSourcing.Net.Samples.CustomIdTypes.IntAggregate;
using EventSourcing.Net.Samples.CustomIdTypes.StringAggregate;
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

List<AddUserCommand> commands = new List<AddUserCommand>()
{
    new AddUserCommand("User1"),
    new AddUserCommand("User2"),
    new AddUserCommand("User3")
};

IEventSourcingCommandBus bus = provider.GetRequiredService<IEventSourcingCommandBus>();

GuidId guidId = GuidId.New();
StringId stringId = new StringId("some-string");
IntId intId = new IntId(777);

foreach (AddUserCommand command in commands)
{
    ICommandExecutionResult<GuidId> guidResult = await bus.Send(guidId, command);
    ICommandExecutionResult<StringId> stringResult = await bus.Send(stringId, command);
    ICommandExecutionResult<IntId> intResult = await bus.Send(intId, command);
    
    Console.WriteLine("Guid result: " + guidResult);
    Console.WriteLine("String result: " + stringResult);
    Console.WriteLine("Int result: " + intResult);
}

AggregateStateLoader<GuidId, GuidUserAggregate, UserState> guidLoader = new AggregateStateLoader<GuidId, GuidUserAggregate, UserState>();
AggregateStateLoader<StringId, StringUserAggregate, UserState> stringLoader = new AggregateStateLoader<StringId, StringUserAggregate, UserState>();
AggregateStateLoader<IntId, IntUserAggregate, UserState> intLoader = new AggregateStateLoader<IntId, IntUserAggregate, UserState>();

Console.WriteLine("Guid state: " + await guidLoader.GetState(guidId));
Console.WriteLine("String state: " + await stringLoader.GetState(stringId));
Console.WriteLine("Int state: " + await intLoader.GetState(intId));