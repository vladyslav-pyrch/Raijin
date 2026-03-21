using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Logic;

public sealed record Negation(ExpressionNode Node) : ExpressionNode
{
    public override string ToString() => $"!{Node}";

    public override IEnumerable<Variable> GetVariables() => Node.GetVariables();
    
    protected internal override int TseitinTransform(List<IEnumerable<int>> clauses, BijectiveDictionary<Variable, int> symbolTable, Func<int> newLiteralId)
    {
        int nodeLiteral = Node.TseitinTransform(clauses, symbolTable, newLiteralId);
        int negationLiteral = newLiteralId();
        
        clauses.Add([negationLiteral, nodeLiteral]);
        clauses.Add([-negationLiteral, -nodeLiteral]);
        
        return negationLiteral;
    }
}