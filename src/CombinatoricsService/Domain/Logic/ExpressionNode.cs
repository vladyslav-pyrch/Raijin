using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Logic;

public abstract record ExpressionNode
{
    public abstract IEnumerable<Variable> GetVariables();

    public SatReduction TseitinTransform(Guid satReductionId)
    {
        var varId = 1;
        BijectiveDictionary<Variable, int> symbolTable = [];
        List<IEnumerable<int>> clauses = [];

        int lastVariable = TseitinTransform(clauses, symbolTable, () => varId++);
        clauses.Add([lastVariable]);

        return new SatReduction(satReductionId, clauses, symbolTable);
    }

    protected internal abstract int TseitinTransform(List<IEnumerable<int>> clauses,
        BijectiveDictionary<Variable, int> symbolTable,
        Func<int> newLiteralId);
}