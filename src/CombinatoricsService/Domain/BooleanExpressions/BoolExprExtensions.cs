namespace Raijin.CombinatoricsService.Domain.BooleanExpressions;

public static class BoolExprExtensions
{
    public static BoolExpr Negated(this BoolExpr expression) => new Not(expression);

    public static BoolExpr Imply(this BoolExpr leftNode, BoolExpr rightNode) => new Imply(leftNode, rightNode);

    public static BoolExpr Xor(this BoolExpr leftNode, BoolExpr rightNode) => new Xor(leftNode, rightNode);

    public static BoolExpr Or(this BoolExpr leftNode, BoolExpr rightNode) => new Or(leftNode, rightNode);

    public static BoolExpr And(this BoolExpr leftNode, BoolExpr rightNode) => new And(leftNode, rightNode);

    public static BoolExpr Equal(this BoolExpr leftNode, BoolExpr rightNode) => new Equal(leftNode, rightNode);

    public static BoolExpr IsTrue(this BoolExpr expression) => new Equal(expression, new ConstExpr(true));

    public static BoolExpr IsFalse(this BoolExpr expression) => new Equal(expression, new ConstExpr(false));
}