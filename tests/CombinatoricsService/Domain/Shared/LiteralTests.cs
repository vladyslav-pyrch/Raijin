using FluentAssertions;
using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Tests.Shared;

public class LiteralTests
{
    [Fact]
    public void GivenNegativeNumber_WhenConstructingLiteral_ThenThrowArgumentOutOfRangeException()
    {
        // Arrange
        int negativeNumber = -1;

        // Act
        Action act = () => _ = new Literal(negativeNumber);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }


    [Fact]
    public void GivenZero_WhenConstructingLiteral_ThenThrowArgumentOutOfRangeException()
    {
        // Arrange
        var zero = 0;

        // Act
        Action act = () => _ = new Literal(zero);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GivenPositiveNumber_WhenConstructingLiteral_ThenPropertiesAreSetCorrectly()
    {
        // Arrange
        var positiveNumber = 5;

        // Act
        var literal = new Literal(positiveNumber);

        // Assert
        literal.Number.Should().Be(positiveNumber);
        literal.IsNegated.Should().BeFalse();
    }
}