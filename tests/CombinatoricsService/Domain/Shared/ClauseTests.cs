using FluentAssertions;
using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Tests.Shared;

public class ClauseTests
{
    [Fact]
    public void GivenNullLiterals_WhenConstructingClause_ThenThrowArgumentException()
    {
        // Arrange
        List<Literal>? nullLiterals = null;

        // Act
        Action act = () => _ = new Clause(nullLiterals!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenEmptyLiterals_WhenConstructingClause_ThenThrowArgumentException()
    {
        // Arrange
        List<Literal> emptyLiterals = [];

        // Act
        Action act = () => _ = new Clause(emptyLiterals);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenLiteralsWithNullElement_WhenConstructingClause_ThenThrowArgumentException()
    {
        // Arrange
        List<Literal?> literalsWithNullElement = [new(number: 1), null];

        // Act
        Action act = () => _ = new Clause(literalsWithNullElement!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenNonEmptyLiterals_WhenConstructingClause_ThenPropertiesAreSetCorrectly()
    {
        // Arrange
        List<Literal> literals = [new(number: 1), new(number: 2, isNegated: true)];

        // Act
        var clause = new Clause(literals);

        // Assert
        clause.Literals.Should().BeEquivalentTo(literals);
    }
}