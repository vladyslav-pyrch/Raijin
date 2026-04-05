using Raijin.SatSolver.Domain.SatProblems;

namespace Raijin.SatSolver.Application.Solver;

public interface ISatSolver
{
    public Task<int[]> Solve(SatProblem problem, CancellationToken cancellationToken);

    public Task<int[]> Solve(SatProblem problem, int timeout, CancellationToken cancellationToken);
}