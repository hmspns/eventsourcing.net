// See https://aka.ms/new-console-template for more information

using System.Reflection;
using EventSourcing.Net;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Bus.Mediatr;
using EventSourcing.Net.Samples.Mediatr.UserAggregate;
using Microsoft.Extensions.DependencyInjection;

Assembly assembly = Assembly.GetExecutingAssembly();

IServiceCollection services = new ServiceCollection();
services.AddMediatR(x =>
{
    x.RegisterServicesFromAssembly(assembly);
});
services.AddEventSourcing(options =>
{
    options.Bus.UseMediatr();
});
IServiceProvider provider = services.BuildServiceProvider();

// start event sourcing engine
await provider.StartEventSourcingEngine();

CreateUserCommand cmd = new CreateUserCommand("Test", DateTime.UtcNow, "123-456-789");
IEventSourcingCommandBus bus = provider.GetRequiredService<IEventSourcingCommandBus>();


ICommandExecutionResult<Guid> result = await bus.Send(Guid.NewGuid(), cmd);

Console.WriteLine(result);
