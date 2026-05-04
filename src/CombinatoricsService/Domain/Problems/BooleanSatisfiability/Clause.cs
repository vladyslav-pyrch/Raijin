namespace Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

public sealed record Clause(IReadOnlyList<Literal> Literals);