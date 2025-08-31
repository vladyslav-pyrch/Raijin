using Raijin.ProblemSolvingService.Domain.Abstractions;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Domain.BooleanFormulas;

public sealed record Disjunction(IBooleanExpression Expression1, IBooleanExpression Expression2) : ValueObject, IBooleanExpression
{
    public IBooleanExpression Desugar() => new Disjunction(Expression1.Desugar(), Expression2.Desugar());

    public SatVariable TseitinTransform(SatProblem satProblem, Dictionary<Variable, SatVariable> symbolTable, Func<SatVariable> newSatVariable)
    {
        SatVariable x = Expression1.TseitinTransform(satProblem, symbolTable, newSatVariable);
        SatVariable y = Expression2.TseitinTransform(satProblem, symbolTable, newSatVariable);
        SatVariable xOrY = newSatVariable();

        satProblem.AddClause(Literal.Affirmed(xOrY), Literal.Negated(x));
        satProblem.AddClause(Literal.Affirmed(xOrY), Literal.Negated(y));
        satProblem.AddClause(Literal.Negated(xOrY), Literal.Affirmed(x), Literal.Affirmed(y));

        return xOrY;
    }
}