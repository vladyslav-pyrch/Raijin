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

    [Fact]
    public void GivenSatProblem_WhenAddingClause_ThenAddsVariablesContainVariablesFromTheClause()
    {
        var satProblem = new SatProblem();

        satProblem.AddClause(new Clause(literals:
            [Literal.FromInteger(1), Literal.FromInteger(-3)]));

        satProblem.Variables.Should().Contain([new SatVariable(1), new SatVariable(3)]);
    }

    [Fact]
    public void GivenSatProblemWithVariables_WhenAddingClauseWithTheSameVariables_ThenVariablesShouldNotContainDuplicates()
    {
        var satProblem = new SatProblem();
        satProblem.AddClause(new Clause(literals:
            [Literal.FromInteger(1), Literal.FromInteger(3)]));

        satProblem.AddClause(new Clause(literals:
            [Literal.FromInteger(1), Literal.FromInteger(2)]));

        satProblem.Variables.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void GivenSatProblemWithVariables_WhenGettingVariableById_ThenReturnsVariableWithPassedId()
    {
        var satProblem = new SatProblem();
        satProblem.AddClause(new Clause(literals:
            [Literal.FromInteger(1), Literal.FromInteger(3)]));

        SatVariable variable = satProblem.GetVariableById(1);

        variable.Id.Should().Be(1);
    }

    [Fact]
    public void GivenSatProblemWithoutVariable_WhenGettingVariableById_ThenThrowsInvalidOperationException()
    {
        var satProblem = new SatProblem();

        Action when = () => satProblem.GetVariableById(1);

        when.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GivenNegativeId_WhenGettingVariableById_ThenThrowsArgumentOutOfRangeException()
    {
        var satProblem = new SatProblem();

        Action when = () => satProblem.GetVariableById(-1);

        when.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GivenZeroAsId_WhenGettingVariableById_ThenThrowsArgumentOutOfRangeException()
    {
        var satProblem = new SatProblem();

        Action when = () => satProblem.GetVariableById(0);

        when.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GivenListOfLiterals_WhenAddingClause_ThenEquivalentClauseIsAdded()
    {
        var satProblem = new SatProblem();

        satProblem.AddClause(Literal.FromInteger(1), Literal.FromInteger(2));

        satProblem.Clauses.Should().ContainEquivalentOf(new Clause(Literal.FromInteger(1), Literal.FromInteger(2)));
    }

    [Fact]
    public void GivenSatProblemWithNoClauses_WhenGettingNumberOfVariables_ReturnsZero()
    {
        var satProblem = new SatProblem();

        int numberOfVariables = satProblem.GetNumberOfVariables();

        numberOfVariables.Should().Be(0);
    }
}