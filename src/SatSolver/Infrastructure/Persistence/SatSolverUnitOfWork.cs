using Raijin.SatSolver.Application.Persistence;

namespace Raijin.SatSolver.Infrastructure.Persistence;

public sealed class SatSolverUnitOfWork(SatSolverDbContext dbContext) : IUnitOfWork
{
    public Task SaveChanges(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}