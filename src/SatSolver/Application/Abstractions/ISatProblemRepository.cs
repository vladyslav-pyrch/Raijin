using Raijin.SatSolver.Domain.SatProblems;

namespace Raijin.SatSolver.Application.Abstractions;

public interface ISatProblemRepository
{
    public Task<Guid> AddAndSaveAsync(SatProblem satProblem, CancellationToken cancellationToken);

    public Task UpdateAsync(SatProblem satProblem, CancellationToken cancellationToken);
}