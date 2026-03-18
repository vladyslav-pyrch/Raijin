namespace Raijin.CombinatoricsService.Infrastructure.Persistence;

public sealed class DbContextResolver(CombinatoricsServiceDbContext scopedContext)
{
    public CombinatoricsServiceDbContext Resolve() =>
        EfCoreTransaction.CurrentDbContext ?? scopedContext;
}



