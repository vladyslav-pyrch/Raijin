using Raijin.SatSolver.Application.Persistence;

namespace Raijin.SatSolver.Infrastructure.Persistence;

public sealed class SatSolverUnitOfWork(SatSolverDbContext dbContext) : IUnitOfWork
{
    public Task Commit(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}