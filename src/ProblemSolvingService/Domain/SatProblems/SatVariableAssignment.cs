using Raijin.ProblemSolvingService.Domain.Abstractions;

namespace Raijin.ProblemSolvingService.Domain.SatProblems;

public sealed record SatVariableAssignment : ValueObject
{
    public SatVariableAssignment(SatVariable satVariable, bool assignment)
    {
        ArgumentNullException.ThrowIfNull(satVariable, nameof(satVariable));

        SatVariable = satVariable;
        Assignment = assignment;
    }

    public static SatVariableAssignment FromInteger(int value) => value < 0 ?
        new SatVariableAssignment(new SatVariable(-value), assignment: false) : new SatVariableAssignment(new SatVariable(value), assignment: true);

    public SatVariable SatVariable { get; }

    public bool Assignment { get; }
}