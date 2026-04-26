using Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;

namespace Raijin.CombinatoricsService.Domain.Problems.VertexColouring;

public record VertexColoringToCspReductionResult(
    CspInstance CspInstance,
    IReadOnlyDictionary<VertexColorAssignment, DecisionVariableStateAssignment> SymbolTable
);

