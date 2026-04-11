using System.Text.Json.Serialization;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

namespace Raijin.CombinatoricsService.Domain.Problems;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(BooleanSatisfiabilitySolution), ProblemTypes.BooleanSatisfiabilityProblem)]
public abstract record Solution;