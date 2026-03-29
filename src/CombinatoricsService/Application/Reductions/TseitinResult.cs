using Raijin.CombinatoricsService.Domain.BooleanExpressions;
using Raijin.CombinatoricsService.Domain.BooleanSatisfiabilityProblems;

namespace Raijin.CombinatoricsService.Application.Reductions;

/// <summary>
///     The result of a Tseitin transformation applied to a <see cref="BoolExpr" /> tree.
///     Contains the equisatisfiable <see cref="BooleanSatisfiabilityInstance" /> and a mapping from
///     source <see cref="BoolVar" /> nodes to their corresponding <see cref="SatVariable" /> identifiers.
/// </summary>
public sealed record TseitinResult(
    BooleanSatisfiabilityInstance Formula,
    IReadOnlyDictionary<BoolVar, SatVariable> VariableMap);