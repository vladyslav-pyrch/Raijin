using Raijin.ProblemSolvingService.Application.Features.Dtos;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.Shared.Responses;

public enum SolvingStatusResponse
{
    Satisfiable,
    Unsatisfiable,
    Indeterminate
}

public static class SolvingStatusResponseHelpers
{
    public static SolvingStatusResponse ToSolvingStatusResponse(this SolvingStatusDto statusDto) => statusDto switch
    {
        SolvingStatusDto.Solvable => SolvingStatusResponse.Satisfiable,
        SolvingStatusDto.Unsolvable => SolvingStatusResponse.Unsatisfiable,
        SolvingStatusDto.Indeterminate => SolvingStatusResponse.Indeterminate,
        _ => throw new ArgumentOutOfRangeException(nameof(statusDto), statusDto, "Unhandled solving status")
    };
}