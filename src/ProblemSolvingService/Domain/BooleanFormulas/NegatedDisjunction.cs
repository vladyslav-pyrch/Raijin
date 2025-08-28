namespace Raijin.ProblemSolvingService.Domain.BooleanFormulas;

public sealed record NegatedDisjunction(IBooleanExpression Expression1, IBooleanExpression Expression2) : IBooleanExpression
{
    public IBooleanExpression Desugar() => new NegatedDisjunction(Expression1.Desugar(), Expression2.Desugar());
}