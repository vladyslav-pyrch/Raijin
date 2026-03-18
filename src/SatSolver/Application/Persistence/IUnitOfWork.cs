namespace Raijin.SatSolver.Application.Persistence;

public interface IUnitOfWork
{
    public Task SaveChanges(CancellationToken cancellationToken);
}