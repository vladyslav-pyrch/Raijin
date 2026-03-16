using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Logic;

public sealed record TrueNode : ExpressionNode
{
    public override IEnumerable<Variable> GetVariables() => [];

    public override string ToString() => "true";

    protected internal override int TseitinTransform(List<Clause> clauses, BijectiveDictionary<Variable, int> symbolTable, Func<int> newLiteralId)
    {
        int trueLiteral = newLiteralId();
        clauses.Add(new Clause(literals: [trueLiteral]));
        return trueLiteral;
    }
}