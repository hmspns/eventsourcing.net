using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using EventSourcing.Abstractions.Contracts;

namespace EventSouring.Benchmark.General;

[MemoryDiagnoser(true)]
[ExceptionDiagnoser]
[SimpleJob(RunStrategy.Throughput, RuntimeMoniker.Net70, iterationCount:20)]
[Config(typeof(Config))]
[MinColumn, MaxColumn, IterationsColumn]
public class CreateAccountsParallelBenchmark
{
    [Params(100, 250, 500)]
    public int AddCount { get; set; }
    
    [Params(3, 10)]
    public int TaskCount { get; set; }
    

    [Benchmark]
    public async Task PgFullNoRedisBusParallel()
    {
        (IEventSourcingCommandBus bus, IEventSourcingEngine engine) = BenchmarkBus.PgFullNoRedisBus;
        AccountCommandsExecutor service = new AccountCommandsExecutor(bus, engine);

        List<Task> tasks = new List<Task>(TaskCount);
        for (int i = 0; i < TaskCount; i++)
        {
            tasks.Add(service.CreateTestAccount(BenchmarkBus.TenantId, AddCount));
        }

        await Task.WhenAll(tasks);
    }
    
    [Benchmark]
    public async Task PgFullRedisBusParallel()
    {
        (IEventSourcingCommandBus bus, IEventSourcingEngine engine) = BenchmarkBus.PgFullRedisBus;
        AccountCommandsExecutor service = new AccountCommandsExecutor(bus, engine);
        
        List<Task> tasks = new List<Task>(TaskCount);
        for (int i = 0; i < TaskCount; i++)
        {
            tasks.Add(service.CreateTestAccount(BenchmarkBus.TenantId, AddCount));
        }

        await Task.WhenAll(tasks);
    }
}