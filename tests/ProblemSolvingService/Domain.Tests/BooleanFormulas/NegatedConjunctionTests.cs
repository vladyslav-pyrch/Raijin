using FluentAssertions;
using NSubstitute;
using Raijin.ProblemSolvingService.Domain.BooleanFormulas;

namespace Raijin.ProblemSolvingService.Domain.Tests.BooleanFormulas;

[Trait("Category", "Unit")]
public class NegatedConjunctionTests
{
    [Fact]
    public void GivenNegatedConjunction_WhenDesugaring_ThenReturnsDesugaredExpressionTree()
    {
        var subExpression = Substitute.For<IBooleanExpression>();
        subExpression.Desugar().Returns(subExpression);
        var subExpression2 = Substitute.For<IBooleanExpression>();
        subExpression2.Desugar().Returns(subExpression2);

        var negatedConjunction = new NegatedConjunction(subExpression, subExpression2);

        IBooleanExpression desugared = negatedConjunction.Desugar();

        desugared.Should().BeOfType<NegatedConjunction>().And.BeEquivalentTo(new NegatedConjunction(
            subExpression,
            subExpression2
        ));
        subExpression.Received(1).Desugar();
        subExpression2.Received(1).Desugar();
    }
}