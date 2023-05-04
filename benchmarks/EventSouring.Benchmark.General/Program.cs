// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using EventSouring.Benchmark.General;

//Trace.Listeners.Add(new ConsoleTraceListener());
//
// BenchmarkRunner.Run<CreateAccountsBenchmark>();
BenchmarkRunner.Run<CreateAccountsParallelBenchmark>();

// CreateAccountsParallelBenchmark benchmark = new CreateAccountsParallelBenchmark();
// benchmark.AddCount = 100;
// benchmark.TaskCount = 5;
// await benchmark.PgFullNoRedisBusParallel();