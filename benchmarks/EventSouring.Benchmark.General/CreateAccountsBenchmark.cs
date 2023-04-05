using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using EventSourcing.Abstractions.Contracts;

namespace EventSouring.Benchmark.General;

[MemoryDiagnoser()]
[SimpleJob(RunStrategy.Monitoring, RuntimeMoniker.Net70, iterationCount:50)]
[MinColumn, MaxColumn, IterationsColumn]
public class CreateAccountsBenchmark
{
    [Benchmark]
    public async Task InMemory()
    {
        (IEventSourcingCommandBus bus, IEventSourcingEngine engine) = BenchmarkBus.InMemoryBus;
        AccountCommandsExecutor service = new AccountCommandsExecutor(bus, engine);
        await service.CreateTestAccount(BenchmarkBus.TenantId);
    }
    
    [Benchmark]
    public async Task PgFullNoRedisBus()
    {
        (IEventSourcingCommandBus bus, IEventSourcingEngine engine) = BenchmarkBus.PgFullNoRedisBus;
        AccountCommandsExecutor service = new AccountCommandsExecutor(bus, engine);
        await service.CreateTestAccount(BenchmarkBus.TenantId);
    }
    
    [Benchmark]
    public async Task PgFullRedisBus()
    {
        (IEventSourcingCommandBus bus, IEventSourcingEngine engine) = BenchmarkBus.PgFullRedisBus;
        AccountCommandsExecutor service = new AccountCommandsExecutor(bus, engine);
        await service.CreateTestAccount(BenchmarkBus.TenantId);
    }
    
    [Benchmark]
    public async Task PgMinNoRedisBus()
    {
        (IEventSourcingCommandBus bus, IEventSourcingEngine engine) = BenchmarkBus.PgMinNoRedisBus;
        AccountCommandsExecutor service = new AccountCommandsExecutor(bus, engine);
        await service.CreateTestAccount(BenchmarkBus.TenantId);
    }
    
    [Benchmark]
    public async Task PgMinRedisBus()
    {
        (IEventSourcingCommandBus bus, IEventSourcingEngine engine) = BenchmarkBus.PgMinRedisBus;
        AccountCommandsExecutor service = new AccountCommandsExecutor(bus, engine);
        await service.CreateTestAccount(BenchmarkBus.TenantId);
    }
}