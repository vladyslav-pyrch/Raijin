using Microsoft.EntityFrameworkCore;
using Raijin.CombinatoricsService.Application.Persistence;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence;

public sealed class CombinatoricsServiceUnitOfWork(
    CombinatoricsServiceDbContext dbContext,
    IDbContextFactory<CombinatoricsServiceDbContext> dbContextFactory
) : IUnitOfWork
{
    public Task SaveChanges(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);

    public async Task<ITransaction> BeginTransaction(CancellationToken cancellationToken)
    {
        CombinatoricsServiceDbContext isolatedContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var transaction = await isolatedContext.Database.BeginTransactionAsync(cancellationToken);
        return new EfCoreTransaction(isolatedContext, transaction);
    }
}