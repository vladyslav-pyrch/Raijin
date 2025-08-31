using Raijin.ProblemSolvingService.Domain.Abstractions;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Domain.BooleanFormulas;

public sealed record Implication(IBooleanExpression Condition, IBooleanExpression Consequence) : ValueObject, IBooleanExpression
{
    public IBooleanExpression Desugar() => new Implication(Condition.Desugar(), Consequence.Desugar());
    public SatVariable TseitinTransform(SatProblem satProblem, Dictionary<Variable, SatVariable> symbolTable, Func<SatVariable> newSatVariable)
    {
        SatVariable x = Condition.TseitinTransform(satProblem, symbolTable, newSatVariable);
        SatVariable y = Consequence.TseitinTransform(satProblem, symbolTable, newSatVariable);
        SatVariable xImplyY = newSatVariable();

        satProblem.AddClause(Literal.Affirmed(xImplyY), Literal.Affirmed(x));
        satProblem.AddClause(Literal.Affirmed(xImplyY), Literal.Negated(y));
        satProblem.AddClause(Literal.Negated(xImplyY), Literal.Negated(x), Literal.Affirmed(y));

        return xImplyY;
    }
}