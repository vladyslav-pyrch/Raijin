using Raijin.ProblemSolvingService.Application.Features.CommonSat.Dtos;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat.Shared.Responses;

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
        SolvingStatusDto.Satisfiable => SolvingStatusResponse.Satisfiable,
        SolvingStatusDto.Unsatisfiable => SolvingStatusResponse.Unsatisfiable,
        SolvingStatusDto.Indeterminate => SolvingStatusResponse.Indeterminate,
        _ => throw new ArgumentOutOfRangeException(nameof(statusDto), statusDto, "Unhandled solving status")
    };
}