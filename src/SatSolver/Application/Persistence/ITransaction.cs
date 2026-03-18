namespace Raijin.SatSolver.Application.Persistence;

/// <summary>
/// Represents an isolated persistence scope with its own database connection and transaction,
/// independent of the ambient MassTransit outbox transaction.
/// Use for durable checkpoints before long-running operations.
/// While active, repositories automatically use this transaction's database connection.
/// Disposing the transaction restores repositories to the default scoped connection.
/// </summary>
public interface ITransaction : IAsyncDisposable
{
    public Task SaveChanges(CancellationToken cancellationToken);

    public Task Commit(CancellationToken cancellationToken);
}


