using Raijin.ProblemSolvingService.Domain.Abstractions;

namespace Raijin.ProblemSolvingService.Domain.BooleanFormulas;

public sealed record Conjunction(IBooleanExpression Expression1, IBooleanExpression Expression2) : ValueObject, IBooleanExpression
{
    public IBooleanExpression Desugar() => new Conjunction(Expression1.Desugar(), Expression2.Desugar());
}