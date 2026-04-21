using Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;

namespace Raijin.CombinatoricsService.Domain.Problems.EdgeColouring;

public record EdgeColoringToCspReductionResult(
    CspInstance CspInstance,
    IReadOnlyDictionary<EdgeColorAssignment, DecisionVariableStateAssignment> SymbolTable);