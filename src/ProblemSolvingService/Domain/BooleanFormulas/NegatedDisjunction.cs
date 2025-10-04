using Raijin.ProblemSolvingService.Domain.SatProblems;
using Raijin.ProblemSolvingService.Domain.Shared;

namespace Raijin.ProblemSolvingService.Domain.BooleanFormulas;

public sealed record NegatedDisjunction(IBooleanExpression Expression1, IBooleanExpression Expression2) : IBooleanExpression
{
    public IBooleanExpression Desugar() => new NegatedDisjunction(Expression1.Desugar(), Expression2.Desugar());

    public SatVariable TseitinTransform(SatProblem satProblem, BijectiveDictionary<Variable, SatVariable> symbolTable, Func<SatVariable> newSatVariable)
    {
        SatVariable x = Expression1.TseitinTransform(satProblem, symbolTable, newSatVariable);
        SatVariable y = Expression2.TseitinTransform(satProblem, symbolTable, newSatVariable);
        SatVariable xNorY = newSatVariable();

        satProblem.AddClause(Literal.Negated(xNorY), Literal.Negated(x));
        satProblem.AddClause(Literal.Negated(xNorY), Literal.Negated(y));
        satProblem.AddClause(Literal.Affirmed(xNorY), Literal.Affirmed(x), Literal.Affirmed(y));
        satProblem.AddClause(Literal.Affirmed(xNorY));

        return xNorY;
    }
}