using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Logic;

public sealed record FalseNode : ExpressionNode
{
    public override IEnumerable<Variable> GetVariables() => [];

    public override string ToString() => "false";

    protected internal override int TseitinTransform(List<IEnumerable<int>> clauses, BijectiveDictionary<Variable, int> symbolTable, Func<int> newLiteralId)
    {
        int falseLiteral = newLiteralId();
        clauses.Add([-falseLiteral]);
        return falseLiteral;
    }
}