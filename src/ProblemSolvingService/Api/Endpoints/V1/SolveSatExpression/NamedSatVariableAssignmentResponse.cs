using Raijin.ProblemSolvingService.Application.Features.SolveSatExpression;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.SolveSatExpression;

public sealed record NamedSatVariableAssignmentResponse(string VariableName, bool Assignment)
{
    public static NamedSatVariableAssignmentResponse From(NamedSatVariableAssignmentDto dto) => new(dto.VariableName, dto.Assignment);
}