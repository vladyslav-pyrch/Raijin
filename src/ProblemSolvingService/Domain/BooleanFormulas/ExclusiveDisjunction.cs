using Raijin.ProblemSolvingService.Domain.Abstractions;

namespace Raijin.ProblemSolvingService.Domain.BooleanFormulas;

public sealed record ExclusiveDisjunction(IBooleanExpression Expression1, IBooleanExpression Expression2) : ValueObject, IBooleanExpression
{
    public IBooleanExpression Desugar()
    {
        var expression1Desugared = Expression1.Desugar();
        var expression2Desugared = Expression2.Desugar();

        return new Disjunction(
            new Conjunction(
                new Negation(expression1Desugared),
                expression2Desugared
            ),
            new Conjunction(
                expression1Desugared,
                new Negation(expression2Desugared)
            )
        );
    }
}