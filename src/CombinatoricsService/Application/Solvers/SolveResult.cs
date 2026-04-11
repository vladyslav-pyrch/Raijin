using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Solvers;

public sealed record SolveResult(Satisfiability Satisfiability, IReadOnlyList<int> Assignment);
