using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Solvers;

public sealed record SatSolverResult(Satisfiability Satisfiability, IReadOnlyList<int> Assignment);