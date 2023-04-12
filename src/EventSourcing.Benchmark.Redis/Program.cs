// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using EventSourcing.Benchmark.Redis;
using EventSourcing.Core;
using BenchmarkSwitcher = EventSourcing.Core.BenchmarkSwitcher;

BenchmarkRunner.Run<RedisSnapshotStoreBenchmark>();

// RedisSnapshotStoreBenchmark benchmark = new RedisSnapshotStoreBenchmark();
// benchmark.IterationsCount = 100;
// await benchmark.ProcessDataOld();