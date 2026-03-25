using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Logic;

public sealed record Disjunction(ExpressionNode LeftNode, ExpressionNode RightNode) : ExpressionNode
{ 
    public override string ToString() => $"({LeftNode} | {RightNode})";

    public override IEnumerable<Variable> GetVariables() => [..LeftNode.GetVariables(), ..RightNode.GetVariables()];
    
    protected internal override int TseitinTransform(List<IEnumerable<int>> clauses, BijectiveDictionary<Variable, int> symbolTable, Func<int> newLiteralId)
    {
        int left = LeftNode.TseitinTransform(clauses, symbolTable, newLiteralId);
        int right = RightNode.TseitinTransform(clauses, symbolTable, newLiteralId);
        int leftOrRight = newLiteralId();
        
        clauses.Add([leftOrRight, -left]);
        clauses.Add([leftOrRight, -right]);
        clauses.Add([-leftOrRight, left, right]);
        
        return leftOrRight;
    }
}