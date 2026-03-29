using Raijin.CombinatoricsService.Application.Persistence;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence;

internal sealed class CombinatoricsServiceUnitOfWork(CombinatoricsServiceDbContext dbContext) : IUnitOfWork
{
    public Task Commit(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
