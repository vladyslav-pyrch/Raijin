namespace Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;

/// <summary>
///     A decision variable with a finite set of named states.
///     Each variable must have at least two states.
/// </summary>
public sealed record DecisionVariable(string Name, IReadOnlyList<string> States);