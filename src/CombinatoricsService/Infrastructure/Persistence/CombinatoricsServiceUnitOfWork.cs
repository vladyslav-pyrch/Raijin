using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Raijin.CombinatoricsService.Application.Persistence;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence;

internal sealed class CombinatoricsServiceUnitOfWork(
    CombinatoricsServiceDbContext dbContext,
    ILogger<CombinatoricsServiceUnitOfWork> logger
) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public async Task BeginTransaction(CancellationToken cancellationToken)
    {
        if (_transaction is not null)
        {
            logger.LogError("Cannot begin transaction because another transaction is already active.");
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        logger.LogDebug("Database transaction starting.");
        _transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        logger.LogDebug("Database transaction started.");
    }

    public async Task Commit(CancellationToken cancellationToken)
    {
        logger.LogDebug("Saving database changes. HasActiveTransaction={HasActiveTransaction}", _transaction is not null);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (_transaction is not null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
            logger.LogDebug("Database transaction committed.");
            return;
        }

        logger.LogDebug("Database changes committed without explicit transaction.");
    }
}
