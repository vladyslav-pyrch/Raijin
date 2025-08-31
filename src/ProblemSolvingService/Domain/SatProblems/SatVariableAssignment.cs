using Raijin.ProblemSolvingService.Domain.Abstractions;

namespace Raijin.ProblemSolvingService.Domain.SatProblems;

public sealed record SatVariableAssignment : ValueObject
{
    public SatVariableAssignment(SatVariable satVariable, bool value)
    {
        ArgumentNullException.ThrowIfNull(satVariable, nameof(satVariable));

        SatVariable = satVariable;
        Value = value;
    }

    public static SatVariableAssignment FromInteger(int value) => value < 0 ?
        new SatVariableAssignment(new SatVariable(-value), value: false) : new SatVariableAssignment(new SatVariable(value), value: true);

    public SatVariable SatVariable { get; }

    public bool Value { get; }
}