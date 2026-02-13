using Raijin.SatSolver.Domain.SatProblems;

namespace Raijin.SatSolver.Application.Abstractions;

public interface ISatSolver
{
    public Task<int[]> SolveAsync(SatProblem problem, CancellationToken cancellationToken);

    public Task<int[]> SolveAsync(SatProblem problem, int timeout, CancellationToken cancellationToken);

}