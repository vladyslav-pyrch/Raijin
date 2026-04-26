using System.Text.Json.Serialization;

namespace Raijin.CombinatoricsService.Domain.BooleanExpressions;

public sealed record Not(BoolExpr Node) : BoolExpr
{
    [JsonIgnore]
    public override int Precedence => 50;

    public override string ToString() => $"~{Node.BracketedIfLowerPrecedenceThan(this)}";

    public override IEnumerable<BoolVar> GetVariables() => Node.GetVariables();
}