using Raijin.ProblemSolvingService.Application.Features.SolveBooleanExpression;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.SolveBooleanExpression;

public record VariableAssignmentResponse(string VariableName, bool Assignment)
{
    public static VariableAssignmentResponse From(VariableAssignmentDto dto) => new(dto.VariableName, dto.Assignment);
}