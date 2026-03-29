namespace Raijin.CombinatoricsService.Domain.BooleanSatisfiabilityProblems;

public sealed record Clause(IReadOnlyList<Literal> Literals)
{
    internal int GetMaxVariableId() => Literals.Max(l => l.Variable.Id);

    internal string ToDimacsFormat() => string.Join(" ", Literals.Select(l => l.ToDimacsFormat())) + " 0";
}