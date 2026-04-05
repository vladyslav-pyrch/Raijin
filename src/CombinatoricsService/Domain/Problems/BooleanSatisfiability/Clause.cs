namespace Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

public sealed record Clause(IReadOnlyList<Literal> Literals)
{
    internal int GetMaxVariableId() => Literals.Max(l => l.Variable.Id);
}