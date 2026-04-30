namespace Raijin.CombinatoricsService.Domain.BooleanExpressions;

public sealed record Imply(BoolExpr Premise, BoolExpr Conclusion) : BoolExpr
{
    public override int Precedence => 10;

    public override string ToString()
        => $"{Premise.BracketedIfLowerPrecedenceThan(this)} -> {Conclusion.BracketedIfLowerPrecedenceThan(this)}";

    public override IEnumerable<BoolVar> GetVariables() => [..Premise.GetVariables(), ..Conclusion.GetVariables()];
}