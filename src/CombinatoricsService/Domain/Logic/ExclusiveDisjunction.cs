using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Logic;

public sealed record ExclusiveDisjunction(ExpressionNode LeftNode, ExpressionNode RightNode) : ExpressionNode 
{
    public override string ToString() => $"({LeftNode} ^ {RightNode})";

    public override IEnumerable<Variable> GetLeaves() => [..LeftNode.GetLeaves(), ..RightNode.GetLeaves()];
    
    protected internal override int TseitinTransform(List<Clause> clauses, BijectiveDictionary<Variable, int> symbolTable, Func<int> newLiteralId)
    {
        int left = LeftNode.TseitinTransform(clauses, symbolTable, newLiteralId);
        int right = RightNode.TseitinTransform(clauses, symbolTable, newLiteralId);
        int leftXorRight = newLiteralId();
        
        clauses.Add(new Clause(literals: [-leftXorRight, left, right]));
        clauses.Add(new Clause(literals: [-leftXorRight, -left, -right]));
        clauses.Add(new Clause(literals: [leftXorRight, -left, right]));
        clauses.Add(new Clause(literals: [leftXorRight, left, -right]));
        
        return leftXorRight;
    }
}