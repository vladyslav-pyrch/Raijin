using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Raijin.SatSolver.Infrastructure.Persistence;

public class MigrationSatSolverDbContextFactory : IDesignTimeDbContextFactory<SatSolverDbContext>
{
    public SatSolverDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<SatSolverDbContext>();

        builder.UseNpgsql(
            "Host=localhost;Port=5432;Database=dev;Username=dev;Password=dev",
            npgsql => npgsql.MigrationsAssembly(typeof(InfrastructureModule).Assembly.FullName)
        );

        return new SatSolverDbContext(builder.Options);
    }
}
