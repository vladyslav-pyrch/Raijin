using Raijin.ProblemSolvingService.Api.Endpoints.V1.Shared.Responses;
using Raijin.ProblemSolvingService.Application.Features.SolveSatExpression;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.SolveSatExpression;

public sealed record SolveSatExpressionResponse(
    SolvingStatusResponse SolvingStatus,
    List<NamedSatVariableAssignmentResponse> VariableAssignments)
{
    public static SolveSatExpressionResponse From(SolveSatExpressionResult result) => new(
        result.SolvingStatus.ToSolvingStatusResponse(),
        result.VariableAssignments.Select(NamedSatVariableAssignmentResponse.From).ToList()
    );
}