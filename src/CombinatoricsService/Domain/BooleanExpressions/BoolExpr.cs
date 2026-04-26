namespace Raijin.CombinatoricsService.Domain.BooleanExpressions;

public abstract record BoolExpr
{
    public abstract int Precedence { get; }

    public abstract IEnumerable<BoolVar> GetVariables();
}