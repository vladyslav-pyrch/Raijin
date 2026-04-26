namespace Raijin.CombinatoricsService.Domain.BooleanExpressions;

public sealed record And(BoolExpr LeftNode, BoolExpr RightNode) : BoolExpr
{
    public override int Precedence => 40;

    public override string ToString()
        => $"{LeftNode.BracketedIfLowerPrecedenceThan(this)} * {RightNode.BracketedIfLowerPrecedenceThan(this)}";

    public override IEnumerable<BoolVar> GetVariables() => [..LeftNode.GetVariables(), ..RightNode.GetVariables()];
}