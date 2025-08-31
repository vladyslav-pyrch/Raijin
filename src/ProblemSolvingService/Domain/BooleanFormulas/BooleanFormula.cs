using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Domain.BooleanFormulas;

public class BooleanFormula(IBooleanExpression expression)
{
    public IBooleanExpression Expression => expression;

    public Dictionary<Variable, SatVariable> TransformToSat(SatProblem satProblem)
    {
        IBooleanExpression desugaredExpression = Expression.Desugar();
        var symbolTable = new Dictionary<Variable, SatVariable>();
        int varId = satProblem.GetNumberOfVariables() + 1;

        desugaredExpression.TseitinTransform(satProblem, symbolTable, newSatVariable: () => new SatVariable(varId++));

        return symbolTable;
    }
}