using System.Text.Json.Serialization;
using Raijin.CombinatoricsService.Domain.Problems.Boolean;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

namespace Raijin.CombinatoricsService.Domain.Problems;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(BooleanSatisfiabilitySolution), ProblemTypes.BooleanSatisfiabilityProblem)]
[JsonDerivedType(typeof(BooleanProblemSolution), ProblemTypes.BooleanProblem)]
public abstract record Solution;