using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Logic;

public sealed record Negation(ExpressionNode Node) : ExpressionNode
{
    public override string ToString() => $"!{Node}";

    public override IEnumerable<LeafNode> GetLeaves() => Node.GetLeaves();
    
    protected internal override int TseitinTransform(List<Clause> clauses, BijectiveDictionary<LeafNode, int> symbolTable, Func<int> newLiteralId)
    {
        int nodeLiteral = Node.TseitinTransform(clauses, symbolTable, newLiteralId);
        int negationLiteral = newLiteralId();
        
        clauses.Add(new Clause(literals: [negationLiteral, nodeLiteral]));
        clauses.Add(new Clause(literals: [-negationLiteral, -nodeLiteral]));
        
        return negationLiteral;
    }
}