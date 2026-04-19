namespace Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

internal record DimacsReductionResult(SatEncoding SatEncoding, IReadOnlyDictionary<SatVariable, int> SymbolTable);