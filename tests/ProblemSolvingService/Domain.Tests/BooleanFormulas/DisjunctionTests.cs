using FluentAssertions;
using NSubstitute;
using Raijin.ProblemSolvingService.Domain.BooleanFormulas;

namespace Raijin.ProblemSolvingService.Domain.Tests.BooleanFormulas;

[Trait("Category", "Unit")]
public class DisjunctionTests
{
    [Fact]
    public void GivenDisjunction_WhenDesugaring_ThenReturnsDesugaredExpressionTree()
    {
        var subExpression = Substitute.For<IBooleanExpression>();
        subExpression.Desugar().Returns(subExpression);
        var subExpression2 = Substitute.For<IBooleanExpression>();
        subExpression2.Desugar().Returns(subExpression2);

        var exclusiveDisjunction = new Disjunction(subExpression, subExpression2);

        IBooleanExpression desugared = exclusiveDisjunction.Desugar();

        desugared.Should().BeOfType<Disjunction>().And.BeEquivalentTo(new Disjunction(
            subExpression,
            subExpression2
        ));
        subExpression.Received(1).Desugar();
        subExpression2.Received(1).Desugar();
    }
}