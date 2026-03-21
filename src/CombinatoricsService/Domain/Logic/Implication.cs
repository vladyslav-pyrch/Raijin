using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Logic;

public sealed record Implication(ExpressionNode Premise, ExpressionNode Conclusion) : ExpressionNode
{
    public override string ToString() => $"({Premise} -> {Conclusion})";

    public override IEnumerable<Variable> GetVariables() => [..Premise.GetVariables(), ..Conclusion.GetVariables()];
    
    protected internal override int TseitinTransform(List<IEnumerable<int>> clauses, BijectiveDictionary<Variable, int> symbolTable, Func<int> newLiteralId)
    {
        int premise = Premise.TseitinTransform(clauses, symbolTable, newLiteralId);
        int conclusion = Conclusion.TseitinTransform(clauses, symbolTable, newLiteralId);
        int implicationLiteral = newLiteralId();
        
        clauses.Add([implicationLiteral, premise]);
        clauses.Add([implicationLiteral, -conclusion]);
        clauses.Add([-implicationLiteral, -premise, conclusion]);
        
        return implicationLiteral;  
    }
}