using Raijin.ProblemSolvingService.Application.Features.CommonSat;
using Raijin.ProblemSolvingService.Infrastructure.CryptoMiniSat;

namespace Raijin.ProblemSolvingService.Infrastructure.Tests.Integration.CryptoMiniSat;

[Trait("Category", "Integration")]
public class CryptominisatSatSolverTests(CryptominisatDockerContainerFixture fixture)
    : SatSolverTests, IClassFixture<CryptominisatDockerContainerFixture>
{
    protected override ISatSolver GetSatSolver(bool withZeroTimeout = false)
    {
        return !withZeroTimeout ? new CryptominisatSatSolver(new Cryptominisat(fixture.Options)) : new CryptominisatSatSolver(new Cryptominisat(fixture.ZeroTimeoutOptions));
    }
}