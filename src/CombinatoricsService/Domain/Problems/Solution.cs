using System.Text.Json.Serialization;
using Raijin.CombinatoricsService.Domain.Problems.Boolean;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;
using Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;
using Raijin.CombinatoricsService.Domain.Problems.EdgeColouring;
using Raijin.CombinatoricsService.Domain.Problems.VertexColouring;

namespace Raijin.CombinatoricsService.Domain.Problems;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(BooleanSatisfiabilitySolution), ProblemTypes.BooleanSatisfiabilityProblem)]
[JsonDerivedType(typeof(BooleanProblemSolution), ProblemTypes.BooleanProblem)]
[JsonDerivedType(typeof(CspSolution), ProblemTypes.ConstraintSatisfiabilityProblem)]
[JsonDerivedType(typeof(EdgeColoringSolution), ProblemTypes.EdgeColoringProblem)]
[JsonDerivedType(typeof(VertexColoringSolution), ProblemTypes.VertexColoringProblem)]
public abstract record Solution;