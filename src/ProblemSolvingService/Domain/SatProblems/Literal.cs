using Raijin.ProblemSolvingService.Domain.Shared;

namespace Raijin.ProblemSolvingService.Domain.SatProblems;

public sealed record Literal: ValueObject
{
    public Literal(Variable variable, bool isNegated = false)
    {
        ArgumentNullException.ThrowIfNull(variable, nameof(variable));

        Variable = variable;
        IsNegated = isNegated;
    }

    public static Literal FromInteger(int value) => value < 0 ?
        new Literal(new Variable(-value), isNegated: true) : new Literal(new Variable(value), isNegated: false);

    public Variable Variable { get; }
    public bool IsNegated { get; }

    internal string ToDimacsString() => IsNegated ? $"-{Variable.ToDimacsString()}" : Variable.ToDimacsString();
}
