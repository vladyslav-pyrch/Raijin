using System.Text.Json.Serialization;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

namespace Raijin.CombinatoricsService.Domain.Problems;

[JsonPolymorphic]
[JsonDerivedType(typeof(BooleanSatisfiabilityInstance), ProblemTypes.BooleanSatisfiabilityProblem)]
public abstract record Instance
{
    [JsonIgnore] public abstract string ProblemType { get; }

    internal abstract SatEncoding ReduceToSat();

    internal abstract Solution InterpretSolution(IReadOnlyList<int> assignment, VariableMap variableMap);
}