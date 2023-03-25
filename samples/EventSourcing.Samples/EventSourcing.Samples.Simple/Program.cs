using System.Reflection;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Net;
using EventSourcing.Samples.Simple.UserAggregate;
using Microsoft.Extensions.DependencyInjection;

Assembly assembly = Assembly.GetExecutingAssembly();

IServiceCollection services = new ServiceCollection();
services
    .AddEventSourcing(options =>
    {
        options.Bus.RegisterImplicitCommandHandlers(assembly);
        options.Bus.RegisterEventConsumers(assembly);
    });
var provider = services.BuildServiceProvider();

CreateUserCommand cmd = new CreateUserCommand("Test", DateTime.UtcNow, "79999999999");
var bus = provider.GetService<IEventSourcingCommandBus>();

var result = await bus.Send(Guid.NewGuid(), cmd);

