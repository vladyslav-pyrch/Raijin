namespace Raijin.CombinatoricsService.Domain.BooleanExpressions;

public sealed record ConstExpr(bool Value) : BoolExpr
{
    public override int Precedence => 60;

    public override IEnumerable<BoolVar> GetVariables() => [];

    public override string ToString() => Value.ToString().ToLower();
}