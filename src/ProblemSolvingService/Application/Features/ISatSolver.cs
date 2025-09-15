using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Features;

public interface ISatSolver
{
    public Task<SatResult> Solve(SatProblem problem, CancellationToken cancellationToken = default);
}