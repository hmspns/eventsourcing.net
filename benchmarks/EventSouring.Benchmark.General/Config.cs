using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;

namespace EventSouring.Benchmark.General;

public class Config : ManualConfig
{
    public Config()
    {
        Add(MemoryDiagnoser.Default);
        Add(ExceptionDiagnoser.Default);
    }
}