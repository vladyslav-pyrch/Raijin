using Microsoft.EntityFrameworkCore;
using Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence;

public class CombinatoricsServiceDbContext(DbContextOptions<CombinatoricsServiceDbContext> options) : DbContext(options)
{
    internal DbSet<StoredEvent> StoredEvents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(InfrastructureModule.Assembly);
    }
}