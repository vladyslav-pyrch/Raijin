using Raijin.ProblemSolvingService.Domain.Abstractions;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Domain.BooleanFormulas;

public sealed record ExclusiveDisjunction(IBooleanExpression Expression1, IBooleanExpression Expression2) : ValueObject, IBooleanExpression
{
    public IBooleanExpression Desugar() => new ExclusiveDisjunction(Expression1.Desugar(), Expression2.Desugar());
    public SatVariable TseitinTransform(SatProblem satProblem, Dictionary<Variable, SatVariable> symbolTable, Func<SatVariable> newSatVariable)
    {
        SatVariable x = Expression1.TseitinTransform(satProblem, symbolTable, newSatVariable);
        SatVariable y = Expression2.TseitinTransform(satProblem, symbolTable, newSatVariable);
        SatVariable xXorY = newSatVariable();

        satProblem.AddClause(Literal.Negated(xXorY), Literal.Negated(y), Literal.Negated(x));
        satProblem.AddClause(Literal.Negated(xXorY), Literal.Affirmed(y), Literal.Affirmed(x));
        satProblem.AddClause(Literal.Affirmed(xXorY), Literal.Negated(y), Literal.Affirmed(x));
        satProblem.AddClause(Literal.Affirmed(xXorY), Literal.Affirmed(y), Literal.Negated(x));

        return xXorY;
    }
}