using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Logic;

public sealed record Conjunction(ExpressionNode LeftNode, ExpressionNode RightNode) : ExpressionNode
{
    public override string ToString()
    {
        return $"({LeftNode} & {RightNode})";
    }

    public override IEnumerable<LeafNode> GetLeaves() => [..LeftNode.GetLeaves(), ..RightNode.GetLeaves()];
    
    protected internal override int TseitinTransform(List<Clause> clauses, BijectiveDictionary<LeafNode, int> symbolTable, Func<int> newLiteralId)
    {
        int left = LeftNode.TseitinTransform(clauses, symbolTable, newLiteralId);
        int right = RightNode.TseitinTransform(clauses, symbolTable, newLiteralId);
        int leftAndRight = newLiteralId();
        
        clauses.Add(new Clause(literals: [-leftAndRight, left]));
        clauses.Add(new Clause(literals: [-leftAndRight, right]));
        clauses.Add(new Clause(literals: [leftAndRight, -left, -right]));
        
        return leftAndRight;
    }
}