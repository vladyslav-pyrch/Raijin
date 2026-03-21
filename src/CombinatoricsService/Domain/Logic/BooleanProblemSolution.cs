using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Logic;

public sealed record BooleanProblemSolution(IEnumerable<VariableAssignment> Assignments);

