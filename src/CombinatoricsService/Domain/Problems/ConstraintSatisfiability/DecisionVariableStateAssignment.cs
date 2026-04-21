using Raijin.CombinatoricsService.Domain.BooleanExpressions;

namespace Raijin.CombinatoricsService.Domain.Problems.ConstraintSatisfiability;

public record DecisionVariableStateAssignment(string Name, string AssignedState)
{
    internal BoolVar ToBoolVar() => new($"{Name}::{AssignedState}");
}