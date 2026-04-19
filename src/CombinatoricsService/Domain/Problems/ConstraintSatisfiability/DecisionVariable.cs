using Raijin.CombinatoricsService.Domain.BooleanExpressions;

namespace Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;

public sealed record DecisionVariable(string Name, IReadOnlyList<string> States)
{
    internal BoolVar[] ToBoolVars() => States.Select(state => new BoolVar($"{Name}::{state}")).ToArray();

    internal BoolVar ToBoolVar(string state)
    {
        if (!States.Contains(state))
            throw new ArgumentException($"State '{state}' is not valid for variable '{Name}'.", nameof(state));

        return new BoolVar($"{Name}::{state}");
    }
    
}