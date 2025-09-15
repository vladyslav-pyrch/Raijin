using Raijin.ProblemSolvingService.Api.Endpoints.V1.Shared.Responses;
using Raijin.ProblemSolvingService.Application.Features.SolveSatProblem;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.SolveSatProblem;

public sealed record SolveSatProblemResponse(
    SolvingStatusResponse SolvingStatus,
    List<VariableAssignmentResponse> VariableAssignments)
{
    public static SolveSatProblemResponse From(SolveSatProblemCommandResult commandResult) =>
        new(
            commandResult.SolvingStatus.ToSolvingStatusResponse(),
            commandResult.VariableAssignments.Select(VariableAssignmentResponse.From).ToList()
        );
}