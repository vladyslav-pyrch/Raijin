using System.Text.Json.Serialization;

namespace Raijin.CombinatoricsService.Domain.BooleanExpressions;

public sealed record Imply(BoolExpr Premise, BoolExpr Conclusion) : BoolExpr
{
    [JsonIgnore]
    public override IReadOnlyList<BoolExpr> Children => [Premise, Conclusion];

    protected override BoolExpr WithChildren(IReadOnlyList<BoolExpr> children) =>
        new Imply(children[0], children[1]);

    protected override int ResolveChildIndex(ChildSelector selector) => selector switch
    {
        ChildSelector.Left => 0,
        ChildSelector.Right => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(selector), selector,
            $"{nameof(Imply)} accepts {nameof(ChildSelector.Left)} or {nameof(ChildSelector.Right)}.")
    };

    public override string ToString() => $"({Premise} -> {Conclusion})";

    public override IEnumerable<BoolVar> GetVariables() => [..Premise.GetVariables(), ..Conclusion.GetVariables()];
}