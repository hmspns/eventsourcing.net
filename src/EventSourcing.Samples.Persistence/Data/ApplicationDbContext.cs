using Microsoft.EntityFrameworkCore;

namespace EventSourcing.Samples.Persistence.Data;

public sealed class AccountDb
{
    public Guid Id { get; set; }
    
    public string OwnerName { get; set; }
    
    public DateTime CreationDate { get; set; }
    
    public DateTime? ClosingDate { get; set; }
}

public sealed class AccountOperationDb
{
    public Guid Id { get; set; }
    
    public Guid AccountId { get; set; }
    
    public bool IsWithdrawn { get; set; }
    
    public decimal Amount { get; set; }
    
    public DateTime OperationDate { get; set; }
}

public sealed class ApplicationDbContext : DbContext
{
    public DbSet<AccountDb> Accounts { get; set; }
    
    public DbSet<AccountOperationDb> AccountOperations { get; set; }
}