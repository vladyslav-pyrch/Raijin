using FluentAssertions;
using Raijin.ProblemSolvingService.Infrastructure.CryptoMiniSat;

namespace Raijin.ProblemSolvingService.Infrastructure.Tests.Integration.CryptoMiniSat;

[Trait("Category", "Integration")]
public sealed class CryptominisatTests(CryptominisatDockerContainerFixture fixture)
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

    [Fact]
    public async Task GivenSolvableProblem_WhenSolving_ThenReturnSolution()
    {
        var cryptominisat = new Cryptominisat(fixture.Options);

        string result = await cryptominisat.Solve(_solvableDimacsProblem);

        result.Should().Be("s SATISFIABLE\nv 1 2 0");
    }

    [Fact]
    public async Task GivenUnsolvableProblem_WhenSolving_ThenReturnUnsolvable()
    {
        var cryptominisat = new Cryptominisat(fixture.Options);

        string result = await cryptominisat.Solve(_unsolvableDimacsProblem);

        result.Should().Be("s UNSATISFIABLE");
    }

    [Fact]
    public async Task GivenBadDimacsProblem_WhenSolving_ThenThrowCryptominisatExceptions()
    {
        var cryptominisat = new Cryptominisat(fixture.Options);

        Func<Task> when =  async () => await cryptominisat.Solve(_badDimacsProblem);

        await when.Should().ThrowAsync<CryptominisatException>();
    }

    [Fact]
    public async Task GivenTimeoutIsExceeded_WhenSolving_ThenReturnsIndeterminate()
    {
        var cryptominisat = new Cryptominisat(fixture.ZeroTimeoutOptions);

        string result = await cryptominisat.Solve(_solvableDimacsProblem);

        result.Should().Be("s INDETERMINATE");
    }
}