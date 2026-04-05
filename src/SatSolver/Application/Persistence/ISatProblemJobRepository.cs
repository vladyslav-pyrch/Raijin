using Raijin.SatSolver.Domain.SatProblems;

namespace Raijin.SatSolver.Application.Persistence;

public interface ISatProblemJobRepository
{
    public Task Add(SatProblem satProblem, CancellationToken cancellationToken);

    public Task<SatProblem?> GetById(Guid id, CancellationToken cancellationToken);

    public Task Update(SatProblem satProblem, CancellationToken cancellationToken);

    public Task<SatProblem?> GetNextPendingAndLock(CancellationToken cancellationToken);
}