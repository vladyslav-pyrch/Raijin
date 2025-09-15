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

        desugaredExpression.TseitinTransform(satProblem, symbolTable, newSatVariable: () => new SatVariable(varId++));

        return symbolTable;
    }
}