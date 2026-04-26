namespace Raijin.CombinatoricsService.Domain.BooleanExpressions;

public sealed record Equal(BoolExpr LeftNode, BoolExpr RightNode) : BoolExpr
{
    public override int Precedence => 0;

    public override string ToString() => $"{LeftNode.BracketedIfLowerPrecedenceThan(this)} <-> {RightNode.BracketedIfLowerPrecedenceThan(this)}";

    public override IEnumerable<BoolVar> GetVariables() => [..LeftNode.GetVariables(), ..RightNode.GetVariables()];
}