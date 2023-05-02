namespace EventSourcing.Net.Core;

public static class BenchmarkSwitcher
{
    public static BenchmarkOption BenchmarkOption { get; set; } = BenchmarkOption.A;
}

public enum BenchmarkOption
{
    A,
    B
}