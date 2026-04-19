using System.Text.Json.Serialization;
using Raijin.CombinatoricsService.Domain.Problems.Boolean;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;
using Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;

namespace Raijin.CombinatoricsService.Domain.Problems;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(BooleanSatisfiabilitySolution), ProblemTypes.BooleanSatisfiabilityProblem)]
[JsonDerivedType(typeof(BooleanProblemSolution), ProblemTypes.BooleanProblem)]
[JsonDerivedType(typeof(CspSolution), ProblemTypes.ConstraintSatisfiabilityProblem)]
public abstract record Solution;