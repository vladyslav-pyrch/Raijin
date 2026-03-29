using System.Text.Json.Serialization;
using Raijin.CombinatoricsService.Domain.BooleanSatisfiabilityProblems;

namespace Raijin.CombinatoricsService.Domain.Problems;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(BooleanSatisfiabilityInstance), "sat")]
public abstract record Instance
{
    [JsonIgnore] public abstract string ProblemKind { get; }

    internal abstract SatEncoding ReduceToSat();

    internal abstract Solution InterpretSolution(IReadOnlyList<int> assignment, VariableMap variableMap);
}