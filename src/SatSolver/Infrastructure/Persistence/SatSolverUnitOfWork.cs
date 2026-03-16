using Raijin.SatSolver.Application.Persistence;

namespace Raijin.SatSolver.Infrastructure.Persistence;

public class SatSolverUnitOfWork(SatSolverDbContext dbContext) : IUnitOfWork
{
    public Task SaveChanges(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}