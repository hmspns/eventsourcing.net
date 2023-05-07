namespace EventSourcing.Net.Samples.Persistence.Data;

public sealed class AccountDb
{
    public Guid Id { get; set; }
    
    public string OwnerName { get; set; }
    
    public DateTime CreationDate { get; set; }
    
    public DateTime? ClosingDate { get; set; }
    
    public List<AccountOperationDb> Operations { get; set; }
}