using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Logic;

public sealed record Conjunction(ExpressionNode LeftNode, ExpressionNode RightNode) : ExpressionNode
{
    public override string ToString()
    {
        return $"({LeftNode} & {RightNode})";
    }

    public override IEnumerable<Variable> GetVariables() => [..LeftNode.GetVariables(), ..RightNode.GetVariables()];
    
    protected internal override int TseitinTransform(List<Clause> clauses, BijectiveDictionary<Variable, int> symbolTable, Func<int> newLiteralId)
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