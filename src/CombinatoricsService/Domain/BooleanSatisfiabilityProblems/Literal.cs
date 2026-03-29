namespace Raijin.CombinatoricsService.Domain.BooleanSatisfiabilityProblems;

public sealed record Literal(SatVariable Variable, bool Negated)
{
    internal string ToDimacsFormat() => Negated ? $"-{Variable.ToDimacsFormat()}" : Variable.ToDimacsFormat();
}