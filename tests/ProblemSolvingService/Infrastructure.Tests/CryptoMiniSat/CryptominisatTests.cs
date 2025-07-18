using FluentAssertions;
using Microsoft.Extensions.Options;
using Njinx.ProblemSolvingService.Infrastructure.CryptoMiniSat;

namespace Njinx.ProblemSolvingService.Infrastructure.Tests.CryptoMiniSat;

[Collection("CryptominisatDockerContainer")]
public sealed class CryptominisatTests
    : IClassFixture<CryptominisatDockerContainerFixture>
{
    private readonly string _solvableDimacsProblem = """
                                                     p cnf 2 2
                                                     -1 2 0
                                                     1 0
                                                     """;

    private readonly string _unsolvableDimacsProblem = """
                                                       p cnf 1 2
                                                       1 0
                                                       -1 0
                                                       """;

    private readonly string _badDimacsProblem = """
                                                p cnf 1 2
                                                 1 3
                                                -1 0
                                                """;

    private CryptominisatDockerContainerFixture _fixture;

    public CryptominisatTests(CryptominisatDockerContainerFixture fixture)
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

    [Fact]
    public async Task GivenContainerDoesNotExist_WhenSolving_ThenThrowInvalidOperationException()
    {
        _fixture.StopAndRemoveContainer();

        var cryptominisat = new Cryptominisat(_fixture.Options);

        Func<Task> when = async () => await cryptominisat.Solve(_solvableDimacsProblem);

        await when.Should().ThrowExactlyAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GivenContainerExistsButIsNotStarted_WhenSolving_ThenThrowInvalidOperationException()
    {
        _fixture.StopContainer();

        var cryptominisat = new Cryptominisat(_fixture.Options);

        Func<Task> when = async () => await cryptominisat.Solve(_solvableDimacsProblem);

        await when.Should().ThrowExactlyAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GivenSolvableProblem_WhenSolving_ThenReturnSolution()
    {
        var cryptominisat = new Cryptominisat(_fixture.Options);

        string result = await cryptominisat.Solve(_solvableDimacsProblem);

        result.Should().Be("s SATISFIABLE\nv 1 2 0");
    }

    [Fact]
    public async Task GivenUnsolvableProblem_WhenSolving_ThenReturnUnsolvable()
    {
        var cryptominisat = new Cryptominisat(_fixture.Options);

        string result = await cryptominisat.Solve(_unsolvableDimacsProblem);

        result.Should().Be("s UNSATISFIABLE");
    }

    [Fact]
    public async Task GivenBadDimacsProblem_WhenSolving_ThenThrowCryptominisatExceptions()
    {
        var cryptominisat = new Cryptominisat(_fixture.Options);

        Func<Task> when =  async () => await cryptominisat.Solve(_badDimacsProblem);

        await when.Should().ThrowAsync<CryptominisatException>();
    }

    [Fact]
    public async Task GivenTimeoutIsExceeded_WhenSolving_ThenReturnsIndeterminate()
    {
        CryptominisatOptions amendedOptionsValue = _fixture.Options.Value with { TimeoutSeconds = 0 };
        IOptions<CryptominisatOptions> amendedOptions = Options.Create(amendedOptionsValue);

        var cryptominisat = new Cryptominisat(amendedOptions);

        string result = await cryptominisat.Solve(_solvableDimacsProblem);

        result.Should().Be("s INDETERMINATE");
    }
}