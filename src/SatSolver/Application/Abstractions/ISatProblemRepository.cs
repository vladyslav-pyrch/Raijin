using Raijin.SatSolver.Domain.SatProblems;

namespace Raijin.SatSolver.Application.Abstractions;

public interface ISatProblemRepository
{
    public Task AddAndSave(SatProblem satProblem, CancellationToken cancellationToken);

    public Task UpdateAndSave(SatProblem satProblem, CancellationToken cancellationToken);
}