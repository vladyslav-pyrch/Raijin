using FluentResults;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Solvers;

public interface ISatSolver
{
    string Name { get; }

    Task<Result<SatSolverResult>> Solve(SatEncoding satEncoding, CancellationToken cancellationToken);
}