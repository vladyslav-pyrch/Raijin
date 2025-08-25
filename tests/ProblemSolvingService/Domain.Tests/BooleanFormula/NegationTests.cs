using FluentAssertions;
using NSubstitute;
using Raijin.ProblemSolvingService.Domain.BooleanFormulas;

namespace Raijin.ProblemSolvingService.Domain.Tests.BooleanFormula;

public sealed class NegationTests
{
    [Fact]
    public void GivenNegation_WhenDesugaring_ReturnsDesugaredExpressionTree()
    {
        var subExpression = Substitute.For<IBooleanExpression>();
        subExpression.Desugar().Returns(subExpression);
        var negation = new Negation(subExpression);

        IBooleanExpression desugared = negation.Desugar();

        desugared.Should().BeEquivalentTo(new Negation(subExpression));
        subExpression.Received(1).Desugar();
    }
}