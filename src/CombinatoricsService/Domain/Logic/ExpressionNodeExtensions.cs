namespace Raijin.CombinatoricsService.Domain.Logic;

public static class ExpressionNodeExtensions
{
    public static Negation Negated(this ExpressionNode expression) => new(expression);

    public static Implication Imply(this ExpressionNode expression1, ExpressionNode expression2) =>
        new(Premise: expression1, Conclusion: expression2);

    public static ExclusiveDisjunction Xor(this ExpressionNode expression1, ExpressionNode expression2) =>
        new(expression1, expression2);

    public static Disjunction Or(this ExpressionNode expression1, ExpressionNode expression2) =>
        new(expression1, expression2);

    public static Conjunction And(this ExpressionNode expression1, ExpressionNode expression2) =>
        new(expression1, expression2);

    public static Equivalence Equal(this ExpressionNode expression1, ExpressionNode expression2) =>
        new(expression1, expression2);
}