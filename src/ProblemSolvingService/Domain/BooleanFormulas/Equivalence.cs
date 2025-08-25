using Raijin.ProblemSolvingService.Domain.Abstractions;

namespace Raijin.ProblemSolvingService.Domain.BooleanFormulas;

public sealed record Equivalence(IBooleanExpression Expression1, IBooleanExpression Expression2) : ValueObject, IBooleanExpression
{
    public IBooleanExpression Desugar() => new Disjunction(
        new Conjunction(
            Expression1.Desugar(),
            Expression2.Desugar()
        ),
        new Conjunction(
            new Negation(Expression1.Desugar()),
            new Negation(Expression2.Desugar())
        )
    );
}