using Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat.Shared.Responses;
using Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblem;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat.SolveSatProblem;

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