using Raijin.CombinatoricsService.Domain.BooleanExpressions;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

namespace Raijin.CombinatoricsService.Domain.Problems.Boolean;

internal record TseitinTransformResult(
    BooleanSatisfiabilityInstance Instance,
    IReadOnlyDictionary<BoolVar, SatVariable> SymbolTable);