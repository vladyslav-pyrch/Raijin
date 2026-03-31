namespace Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

public sealed record BooleanSatisfiabilitySolution(IReadOnlyList<SatVariableAssignment> Assignments)
    : Solution;