using Raijin.ProblemSolvingService.Domain.Abstractions;

namespace Raijin.ProblemSolvingService.Domain.SatProblems;

public sealed record VariableAssignment : ValueObject
{
    public VariableAssignment(SatVariable satVariable, bool value)
    {
        ArgumentNullException.ThrowIfNull(satVariable, nameof(satVariable));

        SatVariable = satVariable;
        Value = value;
    }

    public static VariableAssignment FromInteger(int value) => value < 0 ?
        new VariableAssignment(new SatVariable(-value), value: false) : new VariableAssignment(new SatVariable(value), value: true);

    public SatVariable SatVariable { get; }

    public bool Value { get; }
}