namespace Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

public sealed record SatVariable(int Id)
{
    internal string ToDimacsFormat() => Id.ToString();
}