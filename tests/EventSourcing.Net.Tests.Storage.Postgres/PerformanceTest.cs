namespace EventSourcing.Net.Tests.Storage.Postgres;

using System.Reflection;
using Abstractions.Contracts;
using Engine.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Net.Storage.Postgres;
using SequenceAggregate;

public sealed class PerformanceTest : IDisposable
{
    private readonly DbUtils _utils = new DbUtils();
    private const string DB_NAME = "performance";

    [Fact]
    public async Task Test()
    {
        await _utils.CreateDb(DB_NAME);
        string connectionString = _utils.GetConnectionString(DB_NAME);

        IEventSourcingCommandBus bus = await Initialize(connectionString);

        List<Task> tasks = new List<Task>()
        {
            Execute(1),
            Execute(2),
            Execute(3),
            Execute(4),
            Execute(5),
        };

        await Task.WhenAll(tasks).ConfigureAwait(false);

        await Validate(1);
        await Validate(2);
        await Validate(3);
        await Validate(4);
        await Validate(5);

        async Task Execute(int id)
        {
            for (int i = 0; i < 100; i++)
            {
                await bus.Send(id, new AddItemCommand()).ConfigureAwait(false);
            }
        }

        async Task Validate(int id)
        {
            AggregateStateLoader<int, SequenceAggregate.SequenceAggregate, SequenceState> loader = new AggregateStateLoader<int, SequenceAggregate.SequenceAggregate, SequenceState>();
            SequenceState state = await loader.GetState(id);

            foreach (int value in Enumerable.Range(1, 99))
            {
                int stateValue = state.Items[value - 1];
                Assert.Equal(value, stateValue);
            }
        }
    }

    private async Task<IEventSourcingCommandBus> Initialize(string connectionString)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        IServiceCollection services = new ServiceCollection();
        services.AddEventSourcing(options =>
        {
            options.Bus.RegisterCommandHandlers(assembly);
            options.Bus.RegisterEventConsumers(assembly);
            options.Storage.UsePostgresEventStore(connectionString);
        });
        IServiceProvider provider = services.BuildServiceProvider();

        // start event sourcing engine
        await provider.StartEventSourcingEngine();

        IEventSourcingCommandBus bus = provider.GetRequiredService<IEventSourcingCommandBus>();
        return bus;
    }

    public void Dispose()
    {
        _utils.DropDb(DB_NAME).Wait();
    }
}