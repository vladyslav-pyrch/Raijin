namespace Raijin.CombinatoricsService.Domain.BooleanExpressions;

public sealed record Or(BoolExpr LeftNode, BoolExpr RightNode) : BoolExpr
{
    public override IReadOnlyList<BoolExpr> Children => [LeftNode, RightNode];

    protected override BoolExpr WithChildren(IReadOnlyList<BoolExpr> children) =>
        new Or(children[0], children[1]);

    protected override int ResolveChildIndex(ChildSelector selector) => selector switch
    {
        ChildSelector.Left => 0,
        ChildSelector.Right => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(selector), selector,
            $"{nameof(Or)} accepts {nameof(ChildSelector.Left)} or {nameof(ChildSelector.Right)}.")
    };

    public override string ToString() => $"({LeftNode} | {RightNode})";

    public override IEnumerable<BoolVar> GetVariables() => [..LeftNode.GetVariables(), ..RightNode.GetVariables()];
}