using Raijin.ProblemSolvingService.Domain.Abstractions;

namespace Raijin.ProblemSolvingService.Domain.SatProblems;

public sealed record Literal: ValueObject
{
    public Literal(SatVariable satVariable, bool isNegated = false)
    {
        ArgumentNullException.ThrowIfNull(satVariable, nameof(satVariable));

        SatVariable = satVariable;
        IsNegated = isNegated;
    }

    public static Literal FromInteger(int value)
    {
        return value < 0
            ? new Literal(new SatVariable(-value), isNegated: true)
            : new Literal(new SatVariable(value), isNegated: false);
    }

    public static Literal Negated(SatVariable satVariable) => new(satVariable, isNegated: true);

    public static Literal Affirmed(SatVariable satVariable) => new(satVariable, isNegated: false);

    public SatVariable SatVariable { get; }
    public bool IsNegated { get; }

    internal string ToDimacsString() => IsNegated ? $"-{SatVariable.ToDimacsString()}" : SatVariable.ToDimacsString();
}
