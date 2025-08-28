using FluentAssertions;
using NSubstitute;
using Raijin.ProblemSolvingService.Domain.BooleanFormulas;

namespace Raijin.ProblemSolvingService.Domain.Tests.BooleanFormulas;

[Trait("Category", "Unit")]
public class ExclusiveDisjunctionTests
{
    [Fact]
    public void GivenExclusiveDisjunction_WhenDesugaring_ThenReturnsDesugaredExpressionTree()
    {
        var subExpression = Substitute.For<IBooleanExpression>();
        subExpression.Desugar().Returns(subExpression);
        var subExpression2 = Substitute.For<IBooleanExpression>();
        subExpression2.Desugar().Returns(subExpression2);

        var exclusiveDisjunction = new ExclusiveDisjunction(subExpression, subExpression2);

        IBooleanExpression desugared = exclusiveDisjunction.Desugar();

        desugared.Should().BeOfType<ExclusiveDisjunction>().And.BeEquivalentTo(new ExclusiveDisjunction(
            subExpression,
            subExpression2
        ));
        subExpression.Received(1).Desugar();
        subExpression2.Received(1).Desugar();
    }
}