namespace Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatExpression;

public sealed record NamedSatVariableAssignmentDto(string VariableName, bool Assignment);