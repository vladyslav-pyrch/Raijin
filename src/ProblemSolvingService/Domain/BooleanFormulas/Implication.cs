using Raijin.ProblemSolvingService.Domain.Abstractions;

namespace Raijin.ProblemSolvingService.Domain.BooleanFormulas;

public sealed record Implication(IBooleanExpression Condition, IBooleanExpression Consequence) : ValueObject, IBooleanExpression
{
    public IBooleanExpression Desugar() => new Disjunction(
        new Negation(Condition.Desugar()),
        Consequence.Desugar()
    );
}