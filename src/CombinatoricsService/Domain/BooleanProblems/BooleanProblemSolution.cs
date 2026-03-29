using Raijin.CombinatoricsService.Domain.BooleanExpressions;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Domain.BooleanProblems;

public sealed record BooleanProblemSolution(IReadOnlyDictionary<BoolVar, bool> Assignment) : Solution;