using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Logic;

public abstract record ExpressionNode
{
    public abstract IEnumerable<Variable> GetVariables();
    
    protected internal abstract int TseitinTransform(List<IEnumerable<int>> clauses, BijectiveDictionary<Variable, int> symbolTable, Func<int> newLiteralId);
}