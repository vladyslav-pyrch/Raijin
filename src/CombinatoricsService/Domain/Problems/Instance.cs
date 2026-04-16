using System.Text.Json.Serialization;
using Raijin.CombinatoricsService.Domain.Problems.Boolean;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;


namespace Raijin.CombinatoricsService.Domain.Problems;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(BooleanSatisfiabilityInstance), ProblemTypes.BooleanSatisfiabilityProblem)]
[JsonDerivedType(typeof(BooleanProblemInstance), ProblemTypes.BooleanProblem)]
public abstract record Instance
{
    public abstract string ProblemType();

    internal abstract (SatEncoding SatEncoding, VariableMap VariableMap) ReduceToSat();

    internal abstract Solution InterpretSolution(IReadOnlyList<int> assignment);
}