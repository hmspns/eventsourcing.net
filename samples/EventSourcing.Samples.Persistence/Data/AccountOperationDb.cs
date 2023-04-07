namespace EventSourcing.Samples.Persistence.Data;

public sealed class AccountOperationDb
{
    public Guid Id { get; set; }
    
    public Guid AccountId { get; set; }
    
    public bool IsWithdrawn { get; set; }
    
    public decimal Amount { get; set; }
    
    public DateTime OperationDate { get; set; }
    
    public AccountDb Account { get; set; }
}