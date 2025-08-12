using FluentAssertions;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Domain.Tests.SatProblems;

[Trait("Category", "Unit")]
public class SatProblemTests
{
    [Fact]
    public void GivenListOfLiterals_WhenAddingClause_ClauseIsAdded()
    {
        var satProblem = new SatProblem();
        var clause = new Clause(literals:
        [
            Literal.FromInteger(1),
            Literal.FromInteger(-2),
            Literal.FromInteger(3)
        ]);

        satProblem.AddClause(clause);

        satProblem.Clauses.Should().Contain(clause);
    }

    [Fact]
    public void GivenListOfLiteralsIsNull_WhenAddingClause_ThenThrowArgumentNullException()
    {
        var satProblem = new SatProblem();
        Clause clause = null!;

        Action when = () => satProblem.AddClause(clause);

        when.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenOneOfLiteralsIsNull_WhenAddingClause_ThenThrowArgumentException()
    {
        var satProblem = new SatProblem();
        var clause = new Clause(literals:
        [
            Literal.FromInteger(1),
            Literal.FromInteger(-2),
            null!
        ]);

        Action when = () => satProblem.AddClause(clause);

        when.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenSatProblem_WhenGettingNumberOfVariables_ThenReturnsMaxId()
    {
        var satProblem = new SatProblem();
        var clause1 = new Clause(literals:
        [
            Literal.FromInteger(1),
            Literal.FromInteger(-2),
            Literal.FromInteger(3)
        ]);
        var clause3 = new Clause(literals:
        [
            Literal.FromInteger(1),
            Literal.FromInteger(-4),
            Literal.FromInteger(3)
        ]);
        var clause2 = new Clause(literals:
        [
            Literal.FromInteger(2),
            Literal.FromInteger(4)
        ]);

        satProblem.AddClause(clause1);
        satProblem.AddClause(clause2);
        satProblem.AddClause(clause3);

        int numberOfVariables = satProblem.GetNumberOfVariables();

        numberOfVariables.Should().Be(4);
    }

    [Fact]
    public void GivenSatProblem_WhenGettingNumberOfClauses_ReturnsNumberOfClauses()
    {
        var satProblem = new SatProblem();
        var clause1 = new Clause(literals: [Literal.FromInteger(1)]);
        var clause2 = new Clause(literals: [Literal.FromInteger(3)]);
        var clause3 = new Clause(literals: [Literal.FromInteger(2)]);

        satProblem.AddClause(clause1);
        satProblem.AddClause(clause3);
        satProblem.AddClause(clause2);

        int numberOfClauses = satProblem.GetNumberOfClauses();

        numberOfClauses.Should().Be(3);
    }

    [Fact]
    public void GivenSatProblem_WhenGettingDimacs_ThenReturnsValidDimacsString()
    {
        var satProblem = new SatProblem();
        satProblem.AddClause(new Clause(literals:
            [Literal.FromInteger(-1), Literal.FromInteger(-2), Literal.FromInteger(3)]));
        satProblem.AddClause(new Clause(literals:
            [Literal.FromInteger(-1), Literal.FromInteger(2), Literal.FromInteger(3)]));
        satProblem.AddClause(new Clause(literals:
            [Literal.FromInteger(1), Literal.FromInteger(2), Literal.FromInteger(-4)]));

        string dimacsString = satProblem.ToDimacsString();

        dimacsString.Should().Be("p cnf 4 3\n-1 -2 3 0\n-1 2 3 0\n1 2 -4 0");
    }
}