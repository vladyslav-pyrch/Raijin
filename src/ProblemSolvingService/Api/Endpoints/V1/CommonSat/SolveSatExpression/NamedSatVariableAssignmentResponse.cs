using Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatExpression;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat.SolveSatExpression;

public sealed record NamedSatVariableAssignmentResponse(string VariableName, bool Assignment)
{
    public static NamedSatVariableAssignmentResponse From(NamedSatVariableAssignmentDto dto) => new(dto.VariableName, dto.Assignment);
}