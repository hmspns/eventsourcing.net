// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using EventSouring.Benchmark.General;

BenchmarkRunner.Run<CreateAccountsBenchmark>();
