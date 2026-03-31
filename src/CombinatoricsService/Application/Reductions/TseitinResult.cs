using Raijin.CombinatoricsService.Domain.BooleanExpressions;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

namespace Raijin.CombinatoricsService.Application.Reductions;

public sealed record TseitinResult(
    BooleanSatisfiabilityInstance Formula,
    IReadOnlyDictionary<BoolVar, SatVariable> VariableMap
);