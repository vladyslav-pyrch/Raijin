using System.Text.Json.Serialization;

namespace Raijin.CombinatoricsService.Domain.BooleanExpressions;

public sealed record Equal(BoolExpr LeftNode, BoolExpr RightNode) : BoolExpr
{
    [JsonIgnore]
    public override IReadOnlyList<BoolExpr> Children => [LeftNode, RightNode];

    [JsonIgnore]
    public override int Precedence => 0;

    protected override BoolExpr WithChildren(IReadOnlyList<BoolExpr> children) =>
        new Equal(children[0], children[1]);

    protected override int ResolveChildIndex(ChildSelector selector) => selector switch
    {
        ChildSelector.Left => 0,
        ChildSelector.Right => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(selector), selector,
            $"{nameof(Equal)} accepts {nameof(ChildSelector.Left)} or {nameof(ChildSelector.Right)}.")
    };

    public override string ToString() => $"{LeftNode.BracketedIfLowerPrecedenceThan(this)} <-> {RightNode.BracketedIfLowerPrecedenceThan(this)}";

    public override IEnumerable<BoolVar> GetVariables() => [..LeftNode.GetVariables(), ..RightNode.GetVariables()];
}