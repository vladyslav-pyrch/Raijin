using FluentAssertions;
using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Tests.Shared;

public class SatProblemTests
{
    [Fact]
    public void GivenNullClauses_WhenConstructingSatProblem_ThenThrowArgumentException()
    {
        // Arrange
        List<Clause>? nullClauses = null;

        // Act
        Action act = () => _ = new SatProblem(nullClauses!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenEmptyClauses_WhenConstructingSatProblem_ThenThrowArgumentException()
    {
        // Arrange
        List<Clause> emptyClauses = [];

        // Act
        Action act = () => _ = new SatProblem(emptyClauses);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenClausesWithNullElement_WhenConstructingSatProblem_ThenThrowArgumentException()
    {
        // Arrange
        List<Clause?> clausesWithNullElement = [new(literals: [new Literal(number: 1)]), null];

        // Act
        Action act = () => _ = new SatProblem(clausesWithNullElement!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenNonEmptyClauses_WhenConstructingSatProblem_ThenPropertiesAreSetCorrectly()
    {
        // Arrange
        List<Clause> clauses =
        [
            new(literals: [new Literal(number: 1)]),
            new(literals: [new Literal(number: 1), new Literal(number: 2, isNegated: true)])
        ];

        // Act
        var satProblem = new SatProblem(clauses);

        // Assert
        satProblem.Clauses.Should().BeEquivalentTo(clauses);
    }
}