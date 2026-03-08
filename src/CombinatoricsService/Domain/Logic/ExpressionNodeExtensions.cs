namespace Raijin.CombinatoricsService.Domain.Logic;

public static class ExpressionNodeExtensions
{
    public static ExpressionNode Negated(this ExpressionNode expression) => new Negation(expression);

    public static ExpressionNode Imply(this ExpressionNode leftNode, ExpressionNode rightNode) =>
        new Implication(Premise: leftNode, Conclusion: rightNode);

    public static ExpressionNode Xor(this ExpressionNode leftNode, ExpressionNode rightNode) =>
        new ExclusiveDisjunction(leftNode, rightNode);

    public static ExpressionNode Or(this ExpressionNode leftNode, ExpressionNode rightNode) =>
        new Disjunction(leftNode, rightNode);

    public static ExpressionNode And(this ExpressionNode leftNode, ExpressionNode rightNode) =>
        new Conjunction(leftNode, rightNode);

    public static ExpressionNode Equal(this ExpressionNode leftNode, ExpressionNode rightNode) =>
        new Equivalence(leftNode, rightNode);
    
    public static ExpressionNode IsTrue(this  ExpressionNode expression) =>
        new Equivalence(expression, new TrueNode());
    
    public static ExpressionNode IsFalse(this ExpressionNode expression) =>
        new Equivalence(expression, new FalseNode());
}