using Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat.Shared.Responses;
using Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatExpression;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat.SolveSatExpression;

public sealed record SolveSatExpressionResponse(
    SolvingStatusResponse SolvingStatus,
    List<NamedSatVariableAssignmentResponse> VariableAssignments)
{
    public static SolveSatExpressionResponse From(SolveSatExpressionCommandResult result) => new(
        result.SolvingStatus.ToSolvingStatusResponse(),
        result.VariableAssignments.Select(NamedSatVariableAssignmentResponse.From).ToList()
    );
}