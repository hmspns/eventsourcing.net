// See https://aka.ms/new-console-template for more information


using System.Reflection;
using EventSourcing.Net;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Samples.ProtobufNet.UserAggregate;
using EventSourcing.Net.Serialization.ProtobufNet;
using EventSourcing.Net.Storage.Postgres;
using EventSourcing.Net.Storage.Redis;
using Microsoft.Extensions.DependencyInjection;

const string DB_CONNECTION_STRING = "Host=127.0.0.1;Port=5432;Database=es_events_sample_protobufnet;Username=postgres;Password=P@ssw0rd; Include Error Detail=true;";
const string REDIS_CONNECTION_STRING = "127.0.0.1:6379";

Assembly assembly = Assembly.GetExecutingAssembly();

IServiceCollection services = new ServiceCollection();
services.AddEventSourcing(options =>
{
    options.Bus.RegisterCommandHandlers(assembly);
    options.Bus.RegisterEventConsumers(assembly);
    options.Serialization.UseProtobufNet(); // register protobuf-net serialization
    options.Storage.UsePostgresEventStore(DB_CONNECTION_STRING, configurator =>
    {
        configurator.BinaryDataPostgresType = BinaryDataPostgresType.ByteA; // default format is JSONB, it should by changed for protobuf serialization
    });
    options.Storage.UseRedisSnapshotStore(REDIS_CONNECTION_STRING);
});
IServiceProvider provider = services.BuildServiceProvider();

// start event sourcing engine
await provider.StartEventSourcingEngine();

CreateUserCommand cmd = new CreateUserCommand("Test", DateTime.UtcNow, "123-456-789");
IEventSourcingCommandBus bus = provider.GetRequiredService<IEventSourcingCommandBus>();

ICommandExecutionResult<Guid> result = await bus.Send(Guid.NewGuid(), cmd);

Console.WriteLine(result);