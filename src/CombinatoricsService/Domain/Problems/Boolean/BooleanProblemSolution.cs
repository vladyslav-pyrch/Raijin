using Raijin.CombinatoricsService.Domain.BooleanExpressions;

namespace Raijin.CombinatoricsService.Domain.Problems.Boolean;

public sealed record BooleanProblemSolution(IReadOnlyDictionary<BoolVar, bool> Assignment) : Solution;