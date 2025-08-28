using FluentAssertions;
using NSubstitute;
using Raijin.ProblemSolvingService.Domain.BooleanFormulas;

namespace Raijin.ProblemSolvingService.Domain.Tests.BooleanFormulas;

[Trait("Category", "Unit")]
public class EquivalenceTests
{
    [Fact]
    public void GivenEquivalence_WhenDesugaring_ThenReturnsDesugaredExpressionTree()
    {
        var subExpression = Substitute.For<IBooleanExpression>();
        subExpression.Desugar().Returns(subExpression);
        var subExpression2 = Substitute.For<IBooleanExpression>();
        subExpression2.Desugar().Returns(subExpression2);

        var equivalence = new Equivalence(subExpression, subExpression2);

        IBooleanExpression desugared = equivalence.Desugar();

        desugared.Should().BeOfType<Equivalence>().And.BeEquivalentTo(new Equivalence(
            subExpression,
            subExpression2
        ));
        subExpression.Received(1).Desugar();
        subExpression2.Received(1).Desugar();
    }
}