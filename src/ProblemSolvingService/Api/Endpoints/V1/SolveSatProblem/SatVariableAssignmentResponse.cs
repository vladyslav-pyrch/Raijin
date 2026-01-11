using Raijin.ProblemSolvingService.Application.Features.Dtos;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.SolveSatProblem;

public sealed record SatVariableAssignmentResponse(int VariableNumber, bool Assignment)
{
    public static SatVariableAssignmentResponse From(SatVariableAssignmentDto dto) => new(dto.VariableNumber, dto.Assignment);
}