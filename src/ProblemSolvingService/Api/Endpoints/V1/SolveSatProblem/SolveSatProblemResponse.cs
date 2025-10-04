using Raijin.ProblemSolvingService.Api.Endpoints.V1.Shared.Responses;
using Raijin.ProblemSolvingService.Application.Features.SolveSatProblem;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.SolveSatProblem;

public sealed record SolveSatProblemResponse(SolvingStatusResponse SolvingStatus,
    List<SatVariableAssignmentResponse> VariableAssignments)
{
    public static SolveSatProblemResponse From(SolveSatProblemResult result) =>
        new(
            result.SolvingStatus.ToSolvingStatusResponse(),
            result.VariableAssignments.Select(SatVariableAssignmentResponse.From).ToList()
        );
}