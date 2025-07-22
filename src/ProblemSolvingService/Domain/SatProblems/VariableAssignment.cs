using Raijin.ProblemSolvingService.Domain.SharedKernel;

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
        new VariableAssignment(new Variable(-value), value: true) : new VariableAssignment(new Variable(value), value: false);

    public Variable Variable { get; }

    public bool Value { get; }
}