// See https://aka.ms/new-console-template for more information

using EventSourcing.Net;
using EventSourcing.Samples.Simple.UserAggregate;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello, World!");

IServiceCollection services = new ServiceCollection();
services.AddEventSourcing(options => { });

CreateUserCommand cmd = new CreateUserCommand("Test", DateTime.UtcNow, "79999999999");

