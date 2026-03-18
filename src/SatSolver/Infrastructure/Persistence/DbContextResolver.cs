namespace Raijin.SatSolver.Infrastructure.Persistence;

public sealed class DbContextResolver(SatSolverDbContext scopedContext)
{
    public SatSolverDbContext Resolve() =>
        EfCoreTransaction.CurrentDbContext ?? scopedContext;
}





