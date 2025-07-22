using Raijin.ProblemSolvingService.Application.Features.CommonSat;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Infrastructure.CryptoMiniSat;

public sealed class CryptominisatSatSolver : ISatSolver
{
    public Task<SatResult> Solve(SatProblem problem, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}