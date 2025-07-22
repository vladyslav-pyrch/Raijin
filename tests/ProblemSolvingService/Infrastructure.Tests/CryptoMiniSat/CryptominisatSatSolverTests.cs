using Microsoft.Extensions.Options;
using Raijin.ProblemSolvingService.Application.Features.CommonSat;
using Raijin.ProblemSolvingService.Infrastructure.CryptoMiniSat;

namespace Raijin.ProblemSolvingService.Infrastructure.Tests.CryptoMiniSat;

public class CryptominisatSatSolverTests : SatSolverTests, IClassFixture<CryptominisatDockerContainerFixture>
{
    private readonly CryptominisatDockerContainerFixture _fixture;

    public CryptominisatSatSolverTests(CryptominisatDockerContainerFixture fixture)
    {
        _fixture = fixture;

        bool doesExist = _fixture.DoesContainerExist();

        bool isRunning = _fixture.IsContainerRunning();

        switch (doesExist)
        {
            case true when isRunning:
                return;
            case true:
                _fixture.StartContainer();
                break;
            case false:
                _fixture.BuildDockerImage();
                _fixture.RunContainer();
                break;
        }
    }

    protected override ISatSolver GetSatSolver(bool withZeroTimeout = false)
    {
        if (!withZeroTimeout)
            return new CryptominisatSatSolver(new Cryptominisat(_fixture.Options));

        CryptominisatOptions amendedOptionsValue = _fixture.Options.Value with { TimeoutSeconds = 0 };
        IOptions<CryptominisatOptions> options = Options.Create(amendedOptionsValue);
        return new CryptominisatSatSolver(new Cryptominisat(options));
    }
}