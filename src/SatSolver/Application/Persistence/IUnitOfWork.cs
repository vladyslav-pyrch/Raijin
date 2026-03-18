namespace Raijin.SatSolver.Application.Persistence;

public interface IUnitOfWork
{
    public Task SaveChanges(CancellationToken cancellationToken);

    /// <summary>
    /// Opens an isolated persistence scope with its own database connection,
    /// bypassing the ambient MassTransit outbox transaction.
    /// </summary>
    public Task<ITransaction> BeginTransaction(CancellationToken cancellationToken);
}