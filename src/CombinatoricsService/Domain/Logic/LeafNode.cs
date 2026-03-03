using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Logic;

public abstract record LeafNode : ExpressionNode
{
    public override IEnumerable<LeafNode> GetLeaves() => [this];
    
    protected internal override int TseitinTransform(List<Clause> clauses, BijectiveDictionary<LeafNode, int> symbolTable, Func<int> newLiteralId)
    {
        if (symbolTable.TryGetValue(this, out int varId))
            return varId;
        
        varId = newLiteralId();
        symbolTable.Add(this, varId);

        return varId;
    }
}