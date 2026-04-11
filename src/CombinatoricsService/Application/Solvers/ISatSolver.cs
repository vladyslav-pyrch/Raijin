using FluentResults;
using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Application.Solvers;

public interface ISatSolver
{
    public Task<Result<IReadOnlyList<int>>> Solve(SatEncoding satEncoding, CancellationToken cancellationToken);
}