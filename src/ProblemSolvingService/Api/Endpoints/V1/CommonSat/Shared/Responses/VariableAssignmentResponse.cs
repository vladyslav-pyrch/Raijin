using Raijin.ProblemSolvingService.Application.Features.CommonSat.Dtos;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat.Shared.Responses;

public sealed record VariableAssignmentResponse(int VariableNumber, bool Assignment)
{
    public static VariableAssignmentResponse FromVariableAssignmentDto(SatVariableAssignmentDto dto) => new(dto.VariableNumber, dto.Assignment);
}