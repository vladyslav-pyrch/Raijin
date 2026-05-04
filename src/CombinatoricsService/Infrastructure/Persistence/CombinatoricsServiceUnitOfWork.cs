using Microsoft.EntityFrameworkCore.Storage;
using Raijin.CombinatoricsService.Application.Persistence;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence;

internal sealed class CombinatoricsServiceUnitOfWork(CombinatoricsServiceDbContext dbContext) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public async Task BeginTransaction(CancellationToken cancellationToken)
    {
        if (_transaction is not null)
            throw new InvalidOperationException("A transaction is already in progress.");

        _transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task Commit(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);

        if (_transaction is not null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}