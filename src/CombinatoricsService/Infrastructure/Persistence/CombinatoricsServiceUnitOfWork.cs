using Raijin.CombinatoricsService.Application.Persistence;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence;

public sealed class CombinatoricsServiceUnitOfWork(CombinatoricsServiceDbContext dbContext) : IUnitOfWork
{
    public Task Commit(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}