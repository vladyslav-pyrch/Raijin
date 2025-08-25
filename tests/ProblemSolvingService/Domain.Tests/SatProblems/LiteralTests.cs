using FluentAssertions;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Domain.Tests.SatProblems;

[Trait("Category", "Unit")]
public class LiteralTests
{
    [Fact]
    public void GivenVariableIsNull_WhenCreatingLiteral_ThenThrowArgumentNullException()
    {
        SatVariable satVariable = null!;

        Action when = () => _ = new Literal(satVariable);

        when.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenNegativeInteger_WhenCreatingFromInteger_ThenCreatesNegatedLiteral()
    {
        int value = -1;

        Literal literal = Literal.FromInteger(value);

        literal.Should().Be(new Literal(new SatVariable(1), isNegated: true));
    }

    [Fact]
    public void GivenPositiveInteger_WhenCreatingFromInteger_ThenCreatesNonNegatedLiteral()
    {
        int value = 1;

        Literal literal = Literal.FromInteger(value);

        literal.Should().Be(new Literal(new SatVariable(1), isNegated: false));
    }

    [Fact]
    public void GivenZero_WhenCreatingFromInteger_ThenThrowsArgumentOutOfRangeException()
    {
        int value = 0;

        Action when = () => Literal.FromInteger(value);

        when.Should().Throw<ArgumentOutOfRangeException>();
    }
}