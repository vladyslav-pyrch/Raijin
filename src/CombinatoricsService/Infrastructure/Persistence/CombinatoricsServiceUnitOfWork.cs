using Raijin.CombinatoricsService.Application.Persistence;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence;

public sealed class CombinatoricsServiceUnitOfWork(CombinatoricsServiceDbContext dbContext) : IUnitOfWork
{
    public Task SaveChanges(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}