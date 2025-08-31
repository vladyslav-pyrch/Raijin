using Raijin.ProblemSolvingService.Domain.Abstractions;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Domain.BooleanFormulas;

public sealed record Negation(IBooleanExpression Expression) : ValueObject, IBooleanExpression
{
    public IBooleanExpression Desugar() => new Negation(Expression.Desugar());

    public SatVariable TseitinTransform(SatProblem satProblem, Dictionary<Variable, SatVariable> symbolTable, Func<SatVariable> newSatVariable)
    {
        SatVariable x = Expression.TseitinTransform(satProblem, symbolTable, newSatVariable);
        SatVariable notX = newSatVariable();

        satProblem.AddClause(Literal.Negated(x), Literal.Negated(notX));
        satProblem.AddClause(Literal.Affirmed(x), Literal.Affirmed(notX));

        return notX;
    }
}