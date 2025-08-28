namespace Raijin.ProblemSolvingService.Domain.BooleanFormulas;

public sealed record NegatedConjunction(IBooleanExpression Expression1, IBooleanExpression Expression2) : IBooleanExpression
{
    public IBooleanExpression Desugar() => new NegatedConjunction(Expression1.Desugar(), Expression2.Desugar());
}