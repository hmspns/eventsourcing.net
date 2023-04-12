namespace EventSourcing.Benchmark.Shared.Account;

public record AccountState
{
    public bool IsCreated { get; set; }
    
    public bool IsClosed { get; set; }
    
    public string OwnerName { get; set; }

    public decimal Amount { get; set; }
    
    /// <summary>
    /// Barrier to prevent process command twice.
    /// </summary>
    public DateTime LastOperationTimestamp { get; set; }
}