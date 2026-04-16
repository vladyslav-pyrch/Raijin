using System.Text.Json.Serialization;

namespace Raijin.CombinatoricsService.Domain.BooleanExpressions;

public sealed record Not(BoolExpr Node) : BoolExpr
{
    [JsonIgnore]
    public override IReadOnlyList<BoolExpr> Children => [Node];

    protected override BoolExpr WithChildren(IReadOnlyList<BoolExpr> children) =>
        new Not(children[0]);

    protected override int ResolveChildIndex(ChildSelector selector) => selector switch
    {
        ChildSelector.Operand => 0,
        _ => throw new ArgumentOutOfRangeException(nameof(selector), selector,
            $"{nameof(Not)} accepts only {nameof(ChildSelector.Operand)}.")
    };

    public override string ToString() => $"!{Node}";

    public override IEnumerable<BoolVar> GetVariables() => Node.GetVariables();
}