using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Domain.BooleanSatisfiabilityProblems;

public sealed record BooleanSatisfiabilitySolution(IReadOnlyList<SatVariableAssignment> Assignments)
    : Solution;