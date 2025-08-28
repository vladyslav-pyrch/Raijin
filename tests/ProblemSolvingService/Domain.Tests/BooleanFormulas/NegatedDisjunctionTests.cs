using FluentAssertions;
using NSubstitute;
using Raijin.ProblemSolvingService.Domain.BooleanFormulas;

namespace Raijin.ProblemSolvingService.Domain.Tests.BooleanFormulas;

[Trait("Category", "Unit")]
public class NegatedDisjunctionTests
{
    [Fact]
    public void GivenNegatedDisjunction_WhenDesugaring_ThenReturnsDesugaredExpressionTree()
    {
        var subExpression = Substitute.For<IBooleanExpression>();
        subExpression.Desugar().Returns(subExpression);
        var subExpression2 = Substitute.For<IBooleanExpression>();
        subExpression2.Desugar().Returns(subExpression2);

        var negatedDisjunction = new NegatedDisjunction(subExpression, subExpression2);

        IBooleanExpression desugared = negatedDisjunction.Desugar();

        desugared.Should().BeOfType<NegatedDisjunction>().And.BeEquivalentTo(new NegatedDisjunction(
            subExpression,
            subExpression2
        ));
        subExpression.Received(1).Desugar();
        subExpression2.Received(1).Desugar();
    }
}