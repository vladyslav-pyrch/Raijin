using Raijin.CombinatoricsService.Domain.Problems.Boolean;

namespace Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;

public sealed record CspSolution(
    IReadOnlyList<DecisionVariableAssignment> Configuration,
    IReadOnlyList<BooleanVariableAssignment> AuxiliaryAssignments
) : Solution;
