using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence;

public class MigrationCombinatoricsServiceDbContextFactory : IDesignTimeDbContextFactory<CombinatoricsServiceDbContext>
{
    public CombinatoricsServiceDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<CombinatoricsServiceDbContext>();

        builder.UseNpgsql(
            "Host=localhost;Port=5432;Database=dev;Username=dev;Password=dev",
            npgsql => npgsql.MigrationsAssembly(typeof(InfrastructureModule).Assembly.FullName)
        );

        return new CombinatoricsServiceDbContext(builder.Options);
    }
}