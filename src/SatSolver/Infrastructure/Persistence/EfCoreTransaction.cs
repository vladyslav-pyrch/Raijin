using Microsoft.EntityFrameworkCore.Storage;
using Raijin.SatSolver.Application.Persistence;

namespace Raijin.SatSolver.Infrastructure.Persistence;

internal sealed class EfCoreTransaction : ITransaction
{
    private static readonly AsyncLocal<SatSolverDbContext?> AmbientContext = new();

    private readonly SatSolverDbContext _dbContext;
    private readonly IDbContextTransaction _transaction;

    internal EfCoreTransaction(SatSolverDbContext dbContext, IDbContextTransaction transaction)
    {
        _dbContext = dbContext;
        _transaction = transaction;
        AmbientContext.Value = dbContext;
    }

    internal static SatSolverDbContext? CurrentDbContext => AmbientContext.Value;

    public Task SaveChanges(CancellationToken cancellationToken) => _dbContext.SaveChangesAsync(cancellationToken);

    public Task Commit(CancellationToken cancellationToken) => _transaction.CommitAsync(cancellationToken);

    public async ValueTask DisposeAsync()
    {
        AmbientContext.Value = null;
        await _transaction.DisposeAsync();
        await _dbContext.DisposeAsync();
    }
}


