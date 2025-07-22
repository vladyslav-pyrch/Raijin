using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Features.CommonSat;

public interface ISatSolver
{
    public Task<SatResult> Solve(SatProblem problem, CancellationToken cancellationToken = default);
}