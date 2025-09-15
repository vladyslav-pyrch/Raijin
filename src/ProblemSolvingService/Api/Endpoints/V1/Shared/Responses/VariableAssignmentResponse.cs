using Raijin.ProblemSolvingService.Application.Features.Dtos;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.Shared.Responses;

public sealed record VariableAssignmentResponse(int VariableNumber, bool Assignment)
{
    public static VariableAssignmentResponse From(SatVariableAssignmentDto dto) => new(dto.VariableNumber, dto.Assignment);
}