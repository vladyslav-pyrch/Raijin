namespace Raijin.CombinatoricsService.Domain.Problems.Boolean;

public sealed record BooleanProblemSolution(IReadOnlyList<BooleanVariableAssignment> Assignments) : Solution;