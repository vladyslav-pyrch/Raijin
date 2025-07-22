using FluentAssertions;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Domain.Tests.SatProblems;

public class SatProblemTests
{
    [Fact]
    public void GivenListOfLiterals_WhenAddingClause_ClauseIsAdded()
    {
        var satProblem = new SatProblem();
        List<Literal> literals =
        [
            new(new Variable(1)),
            new(new Variable(2), isNegated: true),
            new(new Variable(3)),
        ];

        satProblem.AddClause(literals);

        satProblem.Clauses.Should().Contain(new Clause(literals));
    }

    [Fact]
    public void GivenListOfLiteralsIsNull_WhenAddingClause_ThenThrowArgumentNullException()
    {
        List<Literal> literals = null!;
        var satProblem = new SatProblem();

        Action when = () => satProblem.AddClause(literals);

        when.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenOneOfLiteralsIsNull_WhenAddingClause_ThenThrowArgumentException()
    {
        List<Literal> literals =
        [
            new(new Variable(1)),
            new(new Variable(2), isNegated: true),
            null!
        ];
        var satProblem = new SatProblem();

        Action when = () => satProblem.AddClause(literals);

        when.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenSatProblem_WhenGettingNumberOfVariables_ThenReturnsMaxId()
    {
        var satProblem = new SatProblem();
        List<Literal> literals1 =
        [
            new(new Variable(1)),
            new(new Variable(2), isNegated: true),
            new(new Variable(3))
        ];
        List<Literal> literals2 =
        [
            new(new Variable(1)),
            new(new Variable(4), isNegated: true),
            new(new Variable(3)),
        ];
        List<Literal> literals3 =
        [
            new(new Variable(2)),
            new(new Variable(4))
        ];

        satProblem.AddClause(literals1);
        satProblem.AddClause(literals3);
        satProblem.AddClause(literals2);

        int numberOfVariables = satProblem.GetNumberOfVariables();

        numberOfVariables.Should().Be(4);
    }

    [Fact]
    public void GivenSatProblem_WhenGettingNumberOfClauses_ReturnsNumberOfClauses()
    {

        var satProblem = new SatProblem();
        List<Literal> literals1 =
        [
            new(new Variable(1))
        ];
        List<Literal> literals2 =
        [
            new(new Variable(3))
        ];
        List<Literal> literals3 =
        [
            new(new Variable(2))
        ];

        satProblem.AddClause(literals1);
        satProblem.AddClause(literals3);
        satProblem.AddClause(literals2);

        int numberOfClauses = satProblem.GetNumberOfClauses();

        numberOfClauses.Should().Be(3);
    }
}