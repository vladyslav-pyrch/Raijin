using System.Text.Json.Serialization;

namespace Raijin.CombinatoricsService.Domain.BooleanExpressions;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(ConstExpr), "const")]
[JsonDerivedType(typeof(BoolVar), "var")]
[JsonDerivedType(typeof(And), "and")]
[JsonDerivedType(typeof(Or), "or")]
[JsonDerivedType(typeof(Not), "not")]
[JsonDerivedType(typeof(Imply), "imply")]
[JsonDerivedType(typeof(Xor), "xor")]
[JsonDerivedType(typeof(Equal), "equal")]
public abstract record BoolExpr
{
    [JsonIgnore]
    public abstract int Precedence { get; }

    public abstract IEnumerable<BoolVar> GetVariables();

}