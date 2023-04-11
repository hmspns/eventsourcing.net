using Microsoft.EntityFrameworkCore;

namespace EventSouring.Benchmark.General;

/// <summary>
/// It's using to create events db only.
/// </summary>
public sealed class EventsDbContext : DbContext
{
    public EventsDbContext(DbContextOptions<EventsDbContext> options) : base(options)
    {
        
    }
}