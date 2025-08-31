using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Domain.BooleanFormulas;

public sealed record NegatedConjunction(IBooleanExpression Expression1, IBooleanExpression Expression2) : IBooleanExpression
{
    public IBooleanExpression Desugar() => new NegatedConjunction(Expression1.Desugar(), Expression2.Desugar());

    public SatVariable TseitinTransform(SatProblem satProblem, Dictionary<Variable, SatVariable> symbolTable, Func<SatVariable> newSatVariable)
    {
        SatVariable x = Expression1.TseitinTransform(satProblem, symbolTable, newSatVariable);
        SatVariable y = Expression2.TseitinTransform(satProblem, symbolTable, newSatVariable);
        SatVariable xNandY = newSatVariable();

        satProblem.AddClause(Literal.Affirmed(xNandY), Literal.Affirmed(x));
        satProblem.AddClause(Literal.Affirmed(xNandY), Literal.Affirmed(y));
        satProblem.AddClause(Literal.Negated(xNandY), Literal.Negated(x), Literal.Negated(y));

        return xNandY;
    }
}