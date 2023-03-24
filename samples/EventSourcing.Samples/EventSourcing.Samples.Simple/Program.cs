// See https://aka.ms/new-console-template for more information

using System.Reflection;
using EventSourcing.Core.InMemory;
using EventSourcing.Net;
using EventSourcing.Samples.Simple.UserAggregate;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello, World!");

IServiceCollection services = new ServiceCollection();
services.AddEventSourcing(options => { });

InMemoryCommandBus.RegisterImplicitHandlers(Assembly.GetExecutingAssembly());

CreateUserCommand cmd = new CreateUserCommand("Test", DateTime.UtcNow, "79999999999");

