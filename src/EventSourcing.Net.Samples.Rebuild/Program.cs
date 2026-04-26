using System.Reflection;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Abstractions.Types;
using EventSourcing.Net.Engine.Implementations;
using EventSourcing.Net.Engine.InMemory;
using EventSourcing.Net.Samples.Rebuild;
using EventSourcing.Net.Samples.Rebuild.UserAggregate;
using EventSourcing.Net.Telemetry;
using Microsoft.Extensions.DependencyInjection;

VirtualStorage virtualStorage = new VirtualStorage();
await virtualStorage.Initialize();

await CreateEvents(virtualStorage);

Console.WriteLine();

await Rebuild(virtualStorage);

async Task CreateEvents(VirtualStorage virtualStorage)
{
    Console.WriteLine("Creating events...");
    
    Assembly assembly = Assembly.GetExecutingAssembly();

    IServiceCollection services = new ServiceCollection();
    virtualStorage.AddServices(services);
    
    services.AddEventSourcing(options =>
    {
        options.Bus.RegisterCommandHandlers(assembly);
        options.Bus.RegisterEventConsumers([typeof(UserProjector)]);
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
}

async Task Rebuild(VirtualStorage virtualStorage)
{
    Console.WriteLine("Rebuilding...");
    Assembly assembly = Assembly.GetExecutingAssembly();

    IServiceCollection services = new ServiceCollection();
    virtualStorage.AddServices(services);
    
    services.AddEventSourcing(options =>
    {
        options.Bus.RegisterCommandHandlers(assembly);
        // use different type of projector for rebuild
        options.Bus.RegisterEventConsumers([typeof(UserRebuildProjector)]);
        options.UsePublicationPreload();
    });
    IServiceProvider provider = services.BuildServiceProvider();

    // start event sourcing engine
    await provider.StartEventSourcingEngine();
    
    IViewsRebuilder rebuilder = provider.GetRequiredService<IViewsRebuilder>();
    await rebuilder.Rebuild();
}