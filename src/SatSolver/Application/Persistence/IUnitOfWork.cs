namespace Raijin.SatSolver.Application.Persistence;

public interface IUnitOfWork
{
    public Task Commit(CancellationToken cancellationToken);
}