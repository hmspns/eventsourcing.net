using System.Diagnostics;
using System.Reflection;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Net;
using EventSourcing.Samples.Simple.UserAggregate;
using Microsoft.Extensions.DependencyInjection;

Trace.Listeners.Add(new ConsoleTraceListener());
Trace.AutoFlush = true;

Assembly assembly = Assembly.GetExecutingAssembly();

IServiceCollection services = new ServiceCollection();
services
    .AddEventSourcing(options =>
    {
        options.Bus.RegisterImplicitCommandHandlers(assembly);
        options.Bus.RegisterEventConsumers(assembly);
    });
ServiceProvider provider = services.BuildServiceProvider();

CreateUserCommand cmd = new CreateUserCommand("Test", DateTime.UtcNow, "79999999999");
IEventSourcingCommandBus bus = provider.GetService<IEventSourcingCommandBus>();

ICommandExecutionResult<Guid> result = await bus.Send(Guid.NewGuid(), cmd);
await bus.PublicationAwaiter.WaitForPublication(result.CommandSequenceId);

Console.WriteLine(result);

