using Raijin.CombinatoricsService.Domain.Problems.Boolean;

namespace Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;

public sealed record CspSolution(
    IReadOnlyList<DecisionVariableStateAssignment> Configuration,
    IReadOnlyList<BooleanVariableAssignment> AuxiliaryAssignments
) : Solution;
