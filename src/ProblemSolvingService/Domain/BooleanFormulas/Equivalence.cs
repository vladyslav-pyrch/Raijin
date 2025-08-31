using Raijin.ProblemSolvingService.Domain.Abstractions;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Domain.BooleanFormulas;

public sealed record Equivalence(IBooleanExpression Expression1, IBooleanExpression Expression2) : ValueObject, IBooleanExpression
{
    public IBooleanExpression Desugar() => new Equivalence(Expression1.Desugar(), Expression2.Desugar());

    public SatVariable TseitinTransform(SatProblem satProblem, Dictionary<Variable, SatVariable> symbolTable, Func<SatVariable> newSatVariable)
    {
        SatVariable x = Expression1.TseitinTransform(satProblem, symbolTable, newSatVariable);
        SatVariable y = Expression2.TseitinTransform(satProblem, symbolTable, newSatVariable);
        SatVariable xEqualY = newSatVariable();

        satProblem.AddClause(Literal.Affirmed(xEqualY), Literal.Negated(y), Literal.Negated(x));
        satProblem.AddClause(Literal.Affirmed(xEqualY), Literal.Affirmed(y), Literal.Affirmed(x));
        satProblem.AddClause(Literal.Negated(xEqualY), Literal.Negated(y), Literal.Affirmed(x));
        satProblem.AddClause(Literal.Negated(xEqualY), Literal.Affirmed(y), Literal.Negated(x));

        return xEqualY;
    }
}