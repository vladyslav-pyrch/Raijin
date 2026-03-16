using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Logic;

public sealed record FalseNode : ExpressionNode
{
    public override IEnumerable<Variable> GetVariables() => [];

    public override string ToString() => "false";

    protected internal override int TseitinTransform(List<Clause> clauses, BijectiveDictionary<Variable, int> symbolTable, Func<int> newLiteralId)
    {
        int falseLiteral = newLiteralId();
        clauses.Add(new Clause(literals: [-falseLiteral]));
        return falseLiteral;
    }
}