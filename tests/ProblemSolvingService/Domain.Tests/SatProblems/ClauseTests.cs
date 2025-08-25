using FluentAssertions;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Domain.Tests.SatProblems;

[Trait("Category", "Unit")]
public class ClauseTests
{
    [Fact]
    public void GivenListOfLiterals_WhenCreatingClause_ThenClauseIsCreated()
    {
        List<Literal> literals =
        [
            new(new SatVariable(1)),
            new(new SatVariable(2), isNegated: true),
            new(new SatVariable(3)),
        ];

        Action when = () => _ = new Clause(literals);

        when.Should().NotThrow();
    }

    [Fact]
    public void GivenListOfLiteralsIsNull_WhenCreatingClause_ThenThrowArgumentNullException()
    {
        List<Literal> literals = null!;

        Action when = () => _ = new Clause(literals);

        when.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenOneOfLiteralsIsNull_WhenCreatingClause_ThenThrowArgumentException()
    {
        List<Literal> literals =
        [
            new(new SatVariable(1)),
            new(new SatVariable(2), isNegated: true),
            null!
        ];

        Action when = () => _ = new Clause(literals);

        when.Should().Throw<ArgumentException>();

    }
}