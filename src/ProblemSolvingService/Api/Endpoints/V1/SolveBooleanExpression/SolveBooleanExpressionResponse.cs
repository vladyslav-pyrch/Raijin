using Raijin.ProblemSolvingService.Api.Endpoints.V1.Shared.Responses;
using Raijin.ProblemSolvingService.Application.Features.SolveBooleanExpression;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.SolveBooleanExpression;

public sealed record SolveBooleanExpressionResponse(SolvingStatusResponse SolvingStatus,
    List<VariableAssignmentResponse> VariableAssignments)
{
    public static SolveBooleanExpressionResponse From(SolveBooleanExpressionResult result) => new(
        result.SolvingStatus.ToSolvingStatusResponse(),
        result.VariableAssignments.Select(VariableAssignmentResponse.From).ToList()
    );
}