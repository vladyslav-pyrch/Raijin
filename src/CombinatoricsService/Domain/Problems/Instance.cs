using System.Text.Json.Serialization;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;


namespace Raijin.CombinatoricsService.Domain.Problems;

[JsonPolymorphic]
[JsonDerivedType(typeof(BooleanSatisfiabilityInstance), ProblemTypes.BooleanSatisfiabilityProblem)]
public abstract record Instance
{
    public abstract string ProblemType();

    internal abstract (SatEncoding SatEncoding, VariableMap VariableMap) ReduceToSat();

    internal abstract Solution InterpretSolution(IReadOnlyList<int> assignment);
}