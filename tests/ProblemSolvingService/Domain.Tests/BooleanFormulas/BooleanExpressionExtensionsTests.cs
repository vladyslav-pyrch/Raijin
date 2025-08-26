using FluentAssertions;
using NSubstitute;
using Raijin.ProblemSolvingService.Domain.BooleanFormulas;

namespace Raijin.ProblemSolvingService.Domain.Tests.BooleanFormulas;

[Trait("Category", "Unit")]
public class BooleanExpressionExtensionsTests
{
    [Fact]
    public void GivenBooleanExpression_WhenNegated_ReturnsNegationOfBooleanExpression()
    {
        var booleanExpression = Substitute.For<IBooleanExpression>();

        Negation negatedBooleanExpression = booleanExpression.Negated();

        negatedBooleanExpression.Should().BeEquivalentTo(new Negation(booleanExpression));
    }

    [Fact]
    public void GivenTwoBooleanExpression_WhenImplying_ReturnsImplication()
    {
        var booleanExpression1 = Substitute.For<IBooleanExpression>();
        var booleanExpression2 = Substitute.For<IBooleanExpression>();

        Implication implication = booleanExpression1.Imply(booleanExpression2);

        implication.Should()
            .BeEquivalentTo(new Implication(Condition: booleanExpression1, Consequence: booleanExpression2));
    }

    [Fact]
    public void GivenTwoBooleanExpression_WhenAnd_ReturnsConjunction()
    {
        var booleanExpression1 = Substitute.For<IBooleanExpression>();
        var booleanExpression2 = Substitute.For<IBooleanExpression>();

        Conjunction conjunction = booleanExpression1.And(booleanExpression2);

        conjunction.Should().BeEquivalentTo(new Conjunction(booleanExpression1, booleanExpression2));
    }

    [Fact]
    public void GivenTwoBooleanExpression_WhenOr_ReturnsDisjunction()
    {
        var booleanExpression1 = Substitute.For<IBooleanExpression>();
        var booleanExpression2 = Substitute.For<IBooleanExpression>();

        Disjunction disjunction = booleanExpression1.Or(booleanExpression2);

        disjunction.Should().BeEquivalentTo(new Disjunction(booleanExpression1, booleanExpression2));
    }

    [Fact]
    public void GivenTwoBooleanExpression_WhenXor_ReturnsExclusiveDisjunction()
    {
        var booleanExpression1 = Substitute.For<IBooleanExpression>();
        var booleanExpression2 = Substitute.For<IBooleanExpression>();

        ExclusiveDisjunction exclusiveDisjunction = booleanExpression1.Xor(booleanExpression2);

        exclusiveDisjunction.Should().BeEquivalentTo(new ExclusiveDisjunction(booleanExpression1, booleanExpression2));
    }

    [Fact]
    public void GivenTwoBooleanExpression_WhenEqualing_ReturnsEquivalence()
    {
        var booleanExpression1 = Substitute.For<IBooleanExpression>();
        var booleanExpression2 = Substitute.For<IBooleanExpression>();

        Equivalence equivalence = booleanExpression1.Equal(booleanExpression2);

        equivalence.Should().BeEquivalentTo(new Equivalence(booleanExpression1, booleanExpression2));
    }
}