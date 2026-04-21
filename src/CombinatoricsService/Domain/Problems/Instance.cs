using System.Text.Json.Serialization;
using Raijin.CombinatoricsService.Domain.Problems.Boolean;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;
using Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;
using Raijin.CombinatoricsService.Domain.Problems.EdgeColouring;
using Raijin.CombinatoricsService.Domain.Problems.VertexColouring;

namespace Raijin.CombinatoricsService.Domain.Problems;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(BooleanSatisfiabilityInstance), ProblemTypes.BooleanSatisfiabilityProblem)]
[JsonDerivedType(typeof(BooleanProblemInstance), ProblemTypes.BooleanProblem)]
[JsonDerivedType(typeof(CspInstance), ProblemTypes.ConstraintSatisfiabilityProblem)]
[JsonDerivedType(typeof(EdgeColoringInstance), ProblemTypes.EdgeColoringProblem)]
[JsonDerivedType(typeof(VertexColoringInstance), ProblemTypes.VertexColoringProblem)]
public abstract record Instance
{
    public abstract string ProblemType();

    internal abstract SatEncoding ReduceToSat();

    internal abstract Solution InterpretSolution(IReadOnlyList<int> assignments);
}