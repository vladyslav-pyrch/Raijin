namespace Raijin.CombinatoricsService.Domain.BooleanSatisfiabilityProblems;

public sealed record SatVariable(int Id)
{
    internal string ToDimacsFormat() => Id.ToString();
}