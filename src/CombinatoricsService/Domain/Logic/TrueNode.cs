using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Logic;

public sealed record TrueNode : ExpressionNode
{
    public override IEnumerable<Variable> GetVariables() => [];

    protected internal override int TseitinTransform(List<Clause> clauses, BijectiveDictionary<Variable, int> symbolTable, Func<int> newLiteralId)
    {
        int trueLiteral = newLiteralId();
        clauses.Add(new Clause(literals: [trueLiteral]));
        return trueLiteral;
    }
}