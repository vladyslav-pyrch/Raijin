using FluentAssertions;
using Njinx.ProblemSolvingService.Domain.SatProblems;

namespace Njinx.ProblemSolvingService.Domain.Tests.SatProblems;

public class ClauseTests
{
    [Fact]
    public void GivenListOfLiterals_WhenCreatingClause_ThenClauseIsCreated()
    {
        List<Literal> literals =
        [
            new(new Variable(1)),
            new(new Variable(2), isNegated: true),
            new(new Variable(3)),
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
            new(new Variable(1)),
            new(new Variable(2), isNegated: true),
            null!
        ];

        Action when = () => _ = new Clause(literals);

        when.Should().Throw<ArgumentException>();

    }
}