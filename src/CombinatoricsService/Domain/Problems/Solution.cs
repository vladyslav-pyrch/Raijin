using System.Text.Json.Serialization;

namespace Raijin.CombinatoricsService.Domain.Problems;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(BooleanSatisfiabilitySolution), "sat")]
public abstract record Solution;