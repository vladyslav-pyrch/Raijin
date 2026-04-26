using System.Text.Json.Serialization;

namespace Raijin.CombinatoricsService.Domain.BooleanExpressions;

public sealed record Or(BoolExpr LeftNode, BoolExpr RightNode) : BoolExpr
{
    [JsonIgnore]
    public override int Precedence => 30;

    public override string ToString() 
        => $"{LeftNode.BracketedIfLowerPrecedenceThan(this)} + {RightNode.BracketedIfLowerPrecedenceThan(this)}";

    public override IEnumerable<BoolVar> GetVariables() => [..LeftNode.GetVariables(), ..RightNode.GetVariables()];
}