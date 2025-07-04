using Njinx.ProblemSolvingService.Domain.SharedKernel;

namespace Njinx.ProblemSolvingService.Domain.SatProblems;

public sealed record Literal: ValueObject
{
    public Literal(Variable variable, bool isNegated = false)
    {
        ArgumentNullException.ThrowIfNull(variable, nameof(variable));

        Variable = variable;
        IsNegated = isNegated;
    }

    public Variable Variable { get; }
    public bool IsNegated { get; }
}
