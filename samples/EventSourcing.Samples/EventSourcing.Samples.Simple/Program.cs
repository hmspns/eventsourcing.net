// See https://aka.ms/new-console-template for more information

using System.Reflection;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Core.InMemory;
using EventSourcing.Net;
using EventSourcing.Samples.Simple.UserAggregate;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello, World!");

IServiceCollection services = new ServiceCollection();
services
    .AddEventSourcing(options =>
    {
        options.RegisterImplicitHandlers(Assembly.GetExecutingAssembly());
    });
var provider = services.BuildServiceProvider();

CreateUserCommand cmd = new CreateUserCommand("Test", DateTime.UtcNow, "79999999999");
var bus = provider.GetService<IEventSourcingCommandBus>();

var result = await bus.Send(Guid.NewGuid(), cmd);

