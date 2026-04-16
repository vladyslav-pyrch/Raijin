using Raijin.CombinatoricsService.Domain.BooleanExpressions;

namespace Raijin.CombinatoricsService.Domain.Problems.Boolean;

public record BooleanVariableAssignment(BoolVar Variable, bool Value);