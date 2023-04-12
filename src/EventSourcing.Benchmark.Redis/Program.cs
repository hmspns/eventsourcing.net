// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using EventSourcing.Benchmark.Redis;

BenchmarkRunner.Run<RedisSnapshotStoreBenchmark>();