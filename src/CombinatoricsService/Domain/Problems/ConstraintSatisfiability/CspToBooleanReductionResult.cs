using Raijin.CombinatoricsService.Domain.BooleanExpressions;
using Raijin.CombinatoricsService.Domain.Problems.Boolean;

namespace Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;

internal record CspToBooleanReductionResult(
    BooleanProblemInstance Instance,
    IReadOnlyDictionary<DecisionVariableAssignment, BoolVar> SymbolTable,
    IReadOnlyList<BoolVar> AuxiliaryVariables
    );