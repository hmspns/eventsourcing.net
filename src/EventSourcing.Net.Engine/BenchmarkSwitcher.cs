namespace EventSourcing.Net.Engine;

public static class BenchmarkSwitcher
{
    public static BenchmarkOption BenchmarkOption { get; set; } = BenchmarkOption.A;
}

public enum BenchmarkOption
{
    A,
    B
}