using Raijin.ProblemSolvingService.Domain.Abstractions;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Domain.BooleanFormulas;

public sealed record Conjunction(IBooleanExpression Expression1, IBooleanExpression Expression2) : ValueObject, IBooleanExpression
{
    public IBooleanExpression Desugar() => new Conjunction(Expression1.Desugar(), Expression2.Desugar());

    public SatVariable TseitinTransform(SatProblem satProblem, Dictionary<Variable, SatVariable> symbolTable, Func<SatVariable> newSatVariable)
    {
        SatVariable x = Expression1.TseitinTransform(satProblem, symbolTable, newSatVariable);
        SatVariable y = Expression2.TseitinTransform(satProblem, symbolTable, newSatVariable);
        SatVariable xAndY = newSatVariable();

        satProblem.AddClause(Literal.Negated(xAndY), Literal.Affirmed(x));
        satProblem.AddClause(Literal.Negated(xAndY), Literal.Affirmed(y));
        satProblem.AddClause(Literal.Affirmed(xAndY), Literal.Negated(x), Literal.Negated(y));

        return xAndY;
    }
}