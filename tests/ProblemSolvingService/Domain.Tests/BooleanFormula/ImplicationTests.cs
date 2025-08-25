using FluentAssertions;
using NSubstitute;
using Raijin.ProblemSolvingService.Domain.BooleanFormulas;

namespace Raijin.ProblemSolvingService.Domain.Tests.BooleanFormula;

public class ImplicationTests
{
    [Fact]
    public void GivenImplication_WhenDesugaring_ThenReturnsDesugaredExpressionTree()
    {
        var subExpression = Substitute.For<IBooleanExpression>();
        subExpression.Desugar().Returns(subExpression);
        var subExpression2 = Substitute.For<IBooleanExpression>();
        subExpression2.Desugar().Returns(subExpression2);

        var implication = new Implication(Condition: subExpression, Consequence: subExpression2);

        IBooleanExpression desugared = implication.Desugar();

        desugared.Should().BeEquivalentTo(new Disjunction(
            new Negation(subExpression),
            subExpression2
        ));
        subExpression.Received(1).Desugar();
        subExpression2.Received(1).Desugar();
    }
}