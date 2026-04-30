namespace Raijin.CombinatoricsService.Domain.BooleanExpressions;

public sealed record Not(BoolExpr Node) : BoolExpr
{
    public override int Precedence => 50;

    public override string ToString() => $"~{Node.BracketedIfLowerPrecedenceThan(this)}";

    public override IEnumerable<BoolVar> GetVariables() => Node.GetVariables();
}