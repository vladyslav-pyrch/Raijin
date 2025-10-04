using Raijin.ProblemSolvingService.Domain.SatProblems;
using Raijin.ProblemSolvingService.Domain.Shared;

namespace Raijin.ProblemSolvingService.Domain.BooleanFormulas;

public sealed class BooleanFormula(IBooleanExpression expression)
{
    public IBooleanExpression Expression => expression;

    public BijectiveDictionary<Variable, SatVariable> TransformToSat(SatProblem satProblem)
    {
        IBooleanExpression desugaredExpression = Expression.Desugar();
        var symbolTable = new BijectiveDictionary<Variable, SatVariable>();
        int varId = satProblem.GetNumberOfVariables() + 1;

        if (Expression is not Variable variable)
            desugaredExpression.TseitinTransform(satProblem, symbolTable, newSatVariable: () => new SatVariable(varId++));
        else
        {
            symbolTable[variable] = new SatVariable(varId);
            satProblem.AddClause(Literal.Affirmed(symbolTable[variable]));
        }

        return symbolTable;
    }
}