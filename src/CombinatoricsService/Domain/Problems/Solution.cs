using System.Text.Json.Serialization;
using Raijin.CombinatoricsService.Domain.BooleanSatisfiabilityProblems;

namespace Raijin.CombinatoricsService.Domain.Problems;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(BooleanSatisfiabilitySolution), "sat")]
public abstract record Solution;