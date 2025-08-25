using Raijin.ProblemSolvingService.Domain.Abstractions;

namespace Raijin.ProblemSolvingService.Domain.BooleanFormulas;

public sealed record Negation(IBooleanExpression Expression) : ValueObject, IBooleanExpression
{
    public IBooleanExpression Desugar() => new Negation(Expression.Desugar());
}