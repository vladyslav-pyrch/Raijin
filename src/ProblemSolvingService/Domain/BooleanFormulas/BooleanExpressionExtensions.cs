namespace Raijin.ProblemSolvingService.Domain.BooleanFormulas;

public static class BooleanExpressionExtensions
{
    public static Negation Negated(this IBooleanExpression expression) => new(expression);

    public static Implication Imply(this IBooleanExpression expression1, IBooleanExpression expression2) =>
        new(Condition: expression1, Consequence: expression2);

    public static ExclusiveDisjunction Xor(this IBooleanExpression expression1, IBooleanExpression expression2) =>
        new(expression1, expression2);

    public static Disjunction Or(this IBooleanExpression expression1, IBooleanExpression expression2) =>
        new(expression1, expression2);

    public static NegatedDisjunction Nor(this IBooleanExpression expression1, IBooleanExpression expression2) =>
        new(expression1, expression2);

    public static Conjunction And(this IBooleanExpression expression1, IBooleanExpression expression2) =>
        new(expression1, expression2);

    public static NegatedConjunction Nand(this IBooleanExpression expression1, IBooleanExpression expression2) =>
        new(expression1, expression2);

    public static Equivalence Equal(this IBooleanExpression expression1, IBooleanExpression expression2) =>
        new(expression1, expression2);
}