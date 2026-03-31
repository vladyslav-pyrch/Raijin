namespace Raijin.CombinatoricsService.Domain.BooleanExpressions;

public sealed record ConstExpr(bool Value) : BoolExpr
{
    public override IReadOnlyList<BoolExpr> Children => [];

    protected override BoolExpr WithChildren(IReadOnlyList<BoolExpr> children) => this;

    protected override int ResolveChildIndex(ChildSelector selector) =>
        throw new InvalidOperationException($"{nameof(ConstExpr)} is a leaf node and has no children.");

    public override IEnumerable<BoolVar> GetVariables() => [];

    public override string ToString() => Value.ToString().ToLower();
}