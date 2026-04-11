using FluentResults;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Solvers;

public interface ISatSolver
{
    public Task<Result<SolveResult>> Solve(SatEncoding satEncoding, CancellationToken cancellationToken);
}