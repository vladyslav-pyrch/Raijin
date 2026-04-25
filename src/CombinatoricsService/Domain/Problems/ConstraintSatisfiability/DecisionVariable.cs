using Raijin.CombinatoricsService.Domain.BooleanExpressions;

namespace Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;

public sealed record DecisionVariable(string Name, IReadOnlyList<string> States)
{
    internal BoolVar[] ToBoolVars() => States.Select(state => new BoolVar($"{Name}::{state}")).ToArray();
}