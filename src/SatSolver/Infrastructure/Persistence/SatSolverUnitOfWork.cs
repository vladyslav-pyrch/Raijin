using Microsoft.EntityFrameworkCore;
using Raijin.SatSolver.Application.Persistence;

namespace Raijin.SatSolver.Infrastructure.Persistence;

public sealed class SatSolverUnitOfWork(
    SatSolverDbContext dbContext,
    IDbContextFactory<SatSolverDbContext> dbContextFactory
) : IUnitOfWork
{
    public Task SaveChanges(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);

    public async Task<ITransaction> BeginTransaction(CancellationToken cancellationToken)
    {
        SatSolverDbContext isolatedContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var transaction = await isolatedContext.Database.BeginTransactionAsync(cancellationToken);
        return new EfCoreTransaction(isolatedContext, transaction);
    }
}