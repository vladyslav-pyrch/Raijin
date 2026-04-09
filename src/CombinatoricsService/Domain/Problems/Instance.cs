using System.Text.Json.Serialization;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;
using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Problems;

[JsonPolymorphic]
[JsonDerivedType(typeof(BooleanSatisfiabilityInstance), ProblemTypes.BooleanSatisfiabilityProblem)]
public abstract record Instance
{
    public abstract string ProblemType();

    internal abstract SatEncoding ReduceToSat();

    internal abstract Solution InterpretSolution(IReadOnlyList<int> assignment, VariableMap variableMap);
}