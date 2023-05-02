using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Benchmark.Shared.Account;
using EventSourcing.Net.Core;
using EventSourcing.Net.Core.Implementations;
using EventSourcing.Net.Core.InMemory;
using EventSourcing.Net.Core.Serialization;
using EventSourcing.Net.Storage.Redis;
using StackExchange.Redis;

namespace EventSourcing.Benchmark.Redis;

[MemoryDiagnoser(true)]
[SimpleJob(RunStrategy.Throughput, RuntimeMoniker.Net70, iterationCount:100)]
[MinColumn, MaxColumn, IterationsColumn]
public class RedisSnapshotStoreBenchmark
{
    private readonly IRedisKeyGenerator _keyGenerator;
    private readonly RedisSnapshotCreationPolicy _redisSnapshotCreationPolicy;
    private readonly ISnapshotsSerializerFactory _serializerFactory = new SystemTextJsonSnapshotsSerializerFactory();
    private readonly TypeMappingHandler _typeMappingHandler;
    private readonly RedisConnection _redisConnection;
    private readonly RedisSnapshotStoreResolver _resolver;

    public RedisSnapshotStoreBenchmark()
    {
        _redisSnapshotCreationPolicy = new RedisSnapshotCreationPolicy()
        {
            Behaviour = RedisSnapshotCreationBehaviour.EveryCommit,
            ExpireAfter = TimeSpan.FromMinutes(5),
        };
        _keyGenerator = new DefaultRedisKeyGenerator(_redisSnapshotCreationPolicy);
        _typeMappingHandler =
            new TypeMappingHandler(new InMemoryTypeMappingStorageProvider(), new DefaultTypeStringConverter());

        _redisConnection = new RedisConnection("127.0.0.1:6379");
        _resolver = new RedisSnapshotStoreResolver(_redisConnection,
            _serializerFactory,
            _keyGenerator,
            _typeMappingHandler,
            _redisSnapshotCreationPolicy);
    }
    
    [Params(100, 500, 1000)]
    public int IterationsCount { get; set; }

    [Benchmark(Description = "Allocation")]
    public async Task ProcessDataOld()
    {
        ISnapshotStore store = _resolver.Get(TenantId.Empty);
        BenchmarkSwitcher.BenchmarkOption = BenchmarkOption.A;
        for (int i = 0; i < IterationsCount; i++)
        {
            StreamId streamName = StreamId.Parse(Guid.NewGuid().ToString());
            await store.SaveSnapshot(streamName, new Snapshot(streamName, new AccountState()
            {
                OwnerName = "test",
                Amount = 100 + i,
                IsClosed = false,
                IsCreated = true,
                LastOperationTimestamp = DateTime.UtcNow
            }, i));
            var result = await store.LoadSnapshot(streamName);
        }
    }
    
    [Benchmark(Description = "No allocation")]
    public async Task ProcessDataNew()
    {
        ISnapshotStore store = _resolver.Get(TenantId.Empty);
        BenchmarkSwitcher.BenchmarkOption = BenchmarkOption.B;
        for (int i = 0; i < IterationsCount; i++)
        {
            StreamId streamName = StreamId.Parse(Guid.NewGuid().ToString());
            await store.SaveSnapshot(streamName, new Snapshot(streamName, new AccountState()
            {
                OwnerName = "test",
                Amount = 100 + i,
                IsClosed = false,
                IsCreated = true,
                LastOperationTimestamp = DateTime.UtcNow
            }, i));
            var result = await store.LoadSnapshot(streamName);
        }
    }
}

internal sealed class DefaultRedisKeyGenerator : IRedisKeyGenerator
{
    private readonly RedisSnapshotCreationPolicy _policy;

    public DefaultRedisKeyGenerator(RedisSnapshotCreationPolicy policy)
    {
        _policy = policy;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RedisKey GetKey(TenantId tenantId, StreamId streamName)
    {
        string prefix = _policy?.KeyPrefix ?? "es";
        RedisKey key;
        if (tenantId != TenantId.Empty)
        {
            key = prefix + "|" + tenantId.Id.ToString() + "|" + streamName;
        }
        else
        {
            key = prefix + "|" + streamName;
        }
        return key;
    }
}