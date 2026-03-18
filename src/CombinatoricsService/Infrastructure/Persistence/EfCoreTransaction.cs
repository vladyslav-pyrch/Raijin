using Microsoft.EntityFrameworkCore.Storage;
using Raijin.CombinatoricsService.Application.Persistence;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence;

internal sealed class EfCoreTransaction : ITransaction
{
    private static readonly AsyncLocal<CombinatoricsServiceDbContext?> AmbientContext = new();

    private readonly CombinatoricsServiceDbContext _dbContext;
    private readonly IDbContextTransaction _transaction;

    internal EfCoreTransaction(CombinatoricsServiceDbContext dbContext, IDbContextTransaction transaction)
    {
        _dbContext = dbContext;
        _transaction = transaction;
        AmbientContext.Value = dbContext;
    }

    internal static CombinatoricsServiceDbContext? CurrentDbContext => AmbientContext.Value;

    public Task SaveChanges(CancellationToken cancellationToken) => _dbContext.SaveChangesAsync(cancellationToken);

    public Task Commit(CancellationToken cancellationToken) => _transaction.CommitAsync(cancellationToken);

    public async ValueTask DisposeAsync()
    {
        AmbientContext.Value = null;
        await _transaction.DisposeAsync();
        await _dbContext.DisposeAsync();
    }
}

