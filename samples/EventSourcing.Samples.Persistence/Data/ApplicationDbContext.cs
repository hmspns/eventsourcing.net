using Microsoft.EntityFrameworkCore;

namespace EventSourcing.Samples.Persistence.Data;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<AccountDb> Accounts { get; set; }
    
    public DbSet<AccountOperationDb> AccountOperations { get; set; }
}

/// <summary>
/// It's using to create events db only.
/// </summary>
public sealed class EventsDbContext : DbContext
{
    public EventsDbContext(DbContextOptions<EventsDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<AccountDb>()
            .HasMany<AccountOperationDb>(x => x.Operations)
            .WithOne(x => x.Account)
            .HasForeignKey(x => x.AccountId);
    }
}