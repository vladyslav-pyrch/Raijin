using Raijin.ProblemSolvingService.Domain.Shared;

namespace Raijin.ProblemSolvingService.Domain.SatProblems;

public sealed record VariableAssignment : ValueObject
{
    public VariableAssignment(Variable variable, bool value)
    {
        ArgumentNullException.ThrowIfNull(variable, nameof(variable));

        Variable = variable;
        Value = value;
    }

    public static VariableAssignment FromInteger(int value) => value < 0 ?
        new VariableAssignment(new Variable(-value), value: false) : new VariableAssignment(new Variable(value), value: true);

    public Variable Variable { get; }

    public bool Value { get; }
}